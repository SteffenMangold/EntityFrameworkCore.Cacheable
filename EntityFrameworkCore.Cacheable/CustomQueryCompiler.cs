using EntityFrameworkCore.Cacheable.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Cacheable
{
    /// <summary>
    /// Extended <see cref="QueryCompiler"/> to handle query caching.
    /// </summary>
    public class CustomQueryCompiler : QueryCompiler
    {
        private readonly IQueryContextFactory _queryContextFactory;
        private readonly ICompiledQueryCache _compiledQueryCache;
        private readonly ICompiledQueryCacheKeyGenerator _compiledQueryCacheKeyGenerator;
        private readonly IDatabase _database;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private readonly IQueryModelGenerator _queryModelGenerator;

        private readonly Type _contextType;
        private readonly CacheProvider _cacheProvider;

        private readonly Func<object, Exception, string> _logFormatter;

        private static MethodInfo ToListMethod { get; }
           = typeof(Enumerable).GetTypeInfo()
               .GetDeclaredMethod(nameof(Enumerable.ToList));

        private static MethodInfo CompileQueryMethod { get; }
            = typeof(IDatabase).GetTypeInfo()
                .GetDeclaredMethod(nameof(IDatabase.CompileQuery));

        public CustomQueryCompiler(IQueryContextFactory queryContextFactory, ICompiledQueryCache compiledQueryCache, ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator
            , IDatabase database, IDiagnosticsLogger<DbLoggerCategory.Query> logger, ICurrentDbContext currentContext, IQueryModelGenerator queryModelGenerator, IEvaluatableExpressionFilter evaluatableExpressionFilter)
            : base(queryContextFactory, compiledQueryCache, compiledQueryCacheKeyGenerator, database, logger, currentContext, queryModelGenerator)
        {
            Check.NotNull(queryContextFactory, nameof(queryContextFactory));
            Check.NotNull(compiledQueryCache, nameof(compiledQueryCache));
            Check.NotNull(compiledQueryCacheKeyGenerator, nameof(compiledQueryCacheKeyGenerator));
            Check.NotNull(database, nameof(database));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(evaluatableExpressionFilter, nameof(evaluatableExpressionFilter));

            _queryContextFactory = queryContextFactory;
            _compiledQueryCache = compiledQueryCache;
            _compiledQueryCacheKeyGenerator = compiledQueryCacheKeyGenerator;
            _database = database;
            _logger = logger;
            _contextType = currentContext.Context.GetType();
            _queryModelGenerator = queryModelGenerator;
            _cacheProvider = new CacheProvider();

            _logFormatter = (queryKey, ex) => $"Cache hit for query [0x{queryKey}] with: {ex?.Message ?? "no error"}";
        }



        public override TResult Execute<TResult>(Expression query)
        {
            Check.NotNull(query, nameof(query));

            var queryContext = _queryContextFactory.Create();

            // search for cacheable operator and extract parameter
            var cachableExpressionVisitor = new CachableExpressionVisitor();
            query = cachableExpressionVisitor.GetExtractCachableParameter(query, out bool isCacheable, out TimeSpan? timeToLive);


            query = _queryModelGenerator.ExtractParameters(_logger, query, queryContext);

            // if cacheable operator is part of the query use cache logic
            if (isCacheable)
            {
                // generate key to identify query
                var queryKey = _cacheProvider.CreateQueryKey(query, queryContext.ParameterValues);

                if (_cacheProvider.TryGetCachedResult<TResult>(queryKey, out TResult cacheResult))
                {
                    _logger.Logger.Log<object>(LogLevel.Debug, CacheableEventId.CacheHit, queryKey, null, _logFormatter);

                    //cache was hit, so return cached query result
                    return cacheResult;
                }
                else // cache was not hit
                {
                    var cacheKey = _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, false);
                    var compiledQuery = _compiledQueryCache.GetOrAddQuery(cacheKey, () => CompileQueryCore<TResult>(query, _queryModelGenerator, _database, _logger, _contextType));

                    // excecute query
                    var queryResult = compiledQuery(queryContext);

                    // add query result to cache
                    _cacheProvider.SetCachedResult<TResult>(queryKey, queryResult, timeToLive.Value);

                    _logger.Logger.Log<object>(LogLevel.Debug, CacheableEventId.QueryResultCached, queryKey, null, _logFormatter);

                    return queryResult;
                }
            }
            else
            {
                // return default query result
                var cacheKey = _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, false);
                var compiledQuery = _compiledQueryCache.GetOrAddQuery(cacheKey, () => CompileQueryCore<TResult>(query, _queryModelGenerator, _database, _logger, _contextType));

                return compiledQuery(queryContext);
            }
        }

        private static Func<QueryContext, TResult> CompileQueryCore<TResult>(
           Expression query,
           IQueryModelGenerator queryModelGenerator,
           IDatabase database,
           IDiagnosticsLogger<DbLoggerCategory.Query> logger,
           Type contextType,
           bool getRealResult = false)
        {
            var queryModel = queryModelGenerator.ParseQuery(query);

            var resultItemType
                = (queryModel.GetOutputDataInfo()
                      as StreamedSequenceInfo)?.ResultItemType
                  ?? typeof(TResult);

            if (resultItemType == typeof(TResult))
            {
                var compiledQuery = database.CompileQuery<TResult>(queryModel);

                return qc =>
                {
                    try
                    {
                        return compiledQuery(qc).First();
                    }
                    catch (Exception exception)
                    {
                        logger.QueryIterationFailed(contextType, exception);

                        throw;
                    }
                };
            }

            try
            {
                // differs from base implmentation to return DB query result
                var compileFunction = (Func<QueryContext, TResult>)CompileQueryMethod
                    .MakeGenericMethod(resultItemType)
                    .Invoke(database, new object[] { queryModel });

                return qc =>
                {
                    try
                    {
                        // calling ToList to materialize result 
                        var genericToListMethod = ToListMethod.MakeGenericMethod(new Type[] { resultItemType });
                        var result = genericToListMethod.Invoke(compileFunction(qc), new object[] { compileFunction(qc) });

                        return (TResult)result;
                    }
                    catch (Exception exception)
                    {
                        logger.QueryIterationFailed(contextType, exception);

                        throw;
                    }
                };
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();

                throw;
            }
        }
    }
}

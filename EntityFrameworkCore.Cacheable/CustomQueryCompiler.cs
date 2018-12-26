using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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

            query = _queryModelGenerator.ExtractParameters(_logger, query, queryContext);

            // search for cacheable operator and extract parameter
            var cachableExpressionVisitor = new CachableExpressionVisitor();
            query = cachableExpressionVisitor.GetExtractCachableParameter(query, out bool isCacheable, out TimeSpan? timeToLive);
            
            // if cacheable operator is part of the query use cache logic
            if (isCacheable)
            {
                // generate key to identify query
                var queryKey = _cacheProvider.CreateQueryKey(query, queryContext.ParameterValues);

                if (_cacheProvider.TryGetCachedResult<TResult>(queryKey, out TResult cacheResult))
                {
                    _logger.Logger.Log<object>(LogLevel.Debug, new EventId(100199, name: "Cache hit"), queryKey, null, _logFormatter);

                    // cache was hit, so return cached query result
                    return cacheResult;
                }
                else // cache was not hit
                {
                    var cacheKey = _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, false);
                    var compiledQuery = _compiledQueryCache.GetOrAddQuery(cacheKey, () => CreateCompiledQuery<TResult>(query));

                    // excecute query
                    var queryResult = compiledQuery(queryContext);

                    // addd query result to cache
                    _cacheProvider.SetCachedResult<TResult>(queryKey, queryResult, timeToLive.Value);

                    return queryResult;
                }
            }
            else
            {
                var cacheKey = _compiledQueryCacheKeyGenerator.GenerateCacheKey(query, false);
                var compiledQuery = _compiledQueryCache.GetOrAddQuery(cacheKey, () => CreateCompiledQuery<TResult>(query));

                return compiledQuery(queryContext);
            }
        }
    }
}

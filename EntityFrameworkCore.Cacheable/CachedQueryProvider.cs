using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Data.HashFunction;
using System.Data.HashFunction.xxHash;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Cacheable
{
    public class CachedQueryProvider<TType> : IQueryProvider
    {
        private readonly IQueryable<TType> _query;
        private static IHashFunction __hashFunction;
        private static readonly Object __syncLock = new Object();
        private TimeSpan _timeToLive;
        private Boolean _isCacheHit;
        private IHashValue _hash;
        private static IMemoryCache __cache = new MemoryCache(new MemoryCacheOptions()
        {
            Clock = new SystemClock(),
            //ExpirationScanFrequency = TimeSpan.FromSeconds(5)
        });

        static CachedQueryProvider()
        {
            __hashFunction = xxHashFactory.Instance.Create(new xxHashConfig
            {
                HashSizeInBits = 64
            });
        }

        /// <summary>
        /// Defines methods to create and execute queries that are described by an System.Linq.IQueryable object.
        /// </summary>
        /// <param name="query">The input EF query.</param>
        /// <param name="saltKey">If you think the computed hash of the query is not enough, set this value.</param>
        /// <param name="debugInfo">Stores the debug information of the caching process.</param>
        /// <param name="cacheKeyProvider">Gets an EF query and returns its hash to store in the cache.</param>
        /// <param name="cacheServiceProvider">The Cache Service Provider.</param>
        public CachedQueryProvider(IQueryable<TType> query, TimeSpan timeToLive)
        {
            _query = query;
            _timeToLive = timeToLive;
        }

        /// <summary>
        /// Stores the debug information of the caching process.
        /// </summary>
        public Boolean IsCacheHit => _isCacheHit;

        public IHashValue Hash => _hash;

        public IQueryable CreateQuery(Expression expression)
        {
            return _query.Provider.CreateQuery(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return _query.Provider.CreateQuery<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return Materialize(expression, () => _query.Provider.Execute(expression));
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Materialize(expression, () => _query.Provider.Execute<TResult>(expression));
        }

        /// <summary>
        /// Executes the query represented by a specified expression tree to cache its results.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <param name="materializer">How to run the query.</param>
        /// <returns>The value that results from executing the specified query.</returns>
        public object Materialize(Expression expression, Func<object> materializer)
        {
            //var sql = _query.ToSql();
            //var sql = expression.ToString();
            //_hash = __hashFunction.ComputeHash(sql);

            //var bla = expressionComparer.GetHashCode(expression);
            //Debug.WriteLine(bla);

            _hash = GetCacheKey(expression);

            //lock (__syncLock)
            //{
            if (__cache.TryGetValue<object>(_hash, out object cacheResult))
            {
                _isCacheHit = true;
                return cacheResult;
            }
            else
            {
                _isCacheHit = false;
                var queryResult = materializer();

                __cache.Set<object>(_hash, queryResult, _timeToLive);
                return queryResult;
            }
            //}
        }

        /// <summary>
        /// Gets a cache key for a query.
        /// </summary>
        public IHashValue GetCacheKey(Expression expression)
        {
            // locally evaluate as much of the query as possible
            expression = Evaluator.PartialEval(expression, CanBeEvaluatedLocally);

            // support local collections
            expression = LocalCollectionExpander.Rewrite(expression);

            // use the string representation of the expression for the cache key
            string key = expression.ToString();

            var hash = __hashFunction.ComputeHash(key);

            return hash;
        }

        Func<Expression, bool> CanBeEvaluatedLocally
        {
            get
            {
                return expression =>
                {
                    // don't evaluate parameters
                    if (expression.NodeType == ExpressionType.Parameter)
                        return false;

                    // can't evaluate queries
                    if (typeof(IQueryable).IsAssignableFrom(expression.Type))
                        return false;

                    return true;
                };
            }
        }
    }
}

using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Data.HashFunction;
using System.Data.HashFunction.xxHash;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Cacheable
{
    /// <summary>
    /// Default CacheProvider based on <see cref="MemoryCache" and <see cref="xxHashFactory"/>./>
    /// </summary>
    public class CacheProvider : ICacheProvider
    {
        private static IHashFunction __hashFunction;
        private static readonly Object __syncLock = new Object();

        private static IMemoryCache __cache = new MemoryCache(new MemoryCacheOptions()
        {
            Clock = new SystemClock()
        });

        /// <summary>
        /// Creates a new <see cref="CacheProvider"/> instance.
        /// </summary>
        static CacheProvider()
        {
            __hashFunction = xxHashFactory.Instance.Create(new xxHashConfig
            {
                // 64 bit size is also used by MS-SQL server to identify queries
                HashSizeInBits = 64
            });
        }

        /// <summary>
        /// Try to get a cached query result from storage.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key">Key to identify query result</param>
        /// <param name="cacheResult">Stored query result if key was found</param>
        /// <returns>Returns <see cref="true"/> when the given key was found <see cref="false"/> otherwise.</returns>
        public Boolean TryGetCachedResult<TResult>(object key, out TResult cacheResult)
        {
            return __cache.TryGetValue<TResult>(key, out cacheResult);
        }

        /// <summary>
        /// Add a query result to cache storage, by given key.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key">Key to identify query result</param>
        /// <param name="value">Query result</param>
        /// <param name="cacheableResultOperator">Options</param>
        public void SetCachedResult<TResult>(object key, TResult value, CacheableResultOperator cacheableResultOperator)
        {
            __cache.Set<TResult>(key, value, cacheableResultOperator.TimeToLive);
        }

        /// <summary>
        /// Creates a unique key to identify a query expression.
        /// </summary>
        /// <param name="expression">Query expression</param>
        /// <param name="parameterValues">Query parameter values</param>
        /// <returns>Unique key object</returns>
        public object CreateQueryKey(Expression expression, IReadOnlyDictionary<string, object> parameterValues)
        {
            // use internal hash cide to identify base query
            var expressionHashCode = ExpressionEqualityComparer.Instance.GetHashCode(expression);

            // creating a Uniform Resource Identifier
            var expressionCacheKey = $"hash://{expressionHashCode}";

            // if query has parameter add key values as uri-query string
            if (parameterValues.Count > 0)
            {
                var parameterStrings = parameterValues.Select(d => $"{d.Key}={d.Value.GetHashCode()}");
                expressionCacheKey += $"?{String.Join("&", parameterStrings)}";
            }

            var hash = __hashFunction.ComputeHash(expressionCacheKey);

            return hash.AsBase64String();
        }


    }
}

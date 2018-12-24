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
    public class CacheProvider
    {
        private static IHashFunction __hashFunction;
        private static readonly Object __syncLock = new Object();

        private static IMemoryCache __cache = new MemoryCache(new MemoryCacheOptions()
        {
            Clock = new SystemClock(),
            //ExpirationScanFrequency = TimeSpan.FromSeconds(5)
        });

        static CacheProvider()
        {
            __hashFunction = xxHashFactory.Instance.Create(new xxHashConfig
            {
                HashSizeInBits = 64
            });
        }

        public Boolean TryGetCachedResult<TResult>(object key, out TResult cacheResult)
        {
            return __cache.TryGetValue<TResult>(key, out cacheResult);
        }

        public void SetCachedResult<TResult>(object key, TResult value, CacheableResultOperator cacheableResultOperator)
        {
            __cache.Set<TResult>(key, value, cacheableResultOperator.TimeToLive);
        }

        public object CreateQueryKey(Expression expression, IReadOnlyDictionary<string, object> parameterValues)
        {
            var expressionHashCode = ExpressionEqualityComparer.Instance.GetHashCode(expression);


            var expressionCacheKey = $"hash://{expressionHashCode}";

            if (parameterValues.Count > 0)
            {
                var parameterStrings = parameterValues.Select(d => $"{d.Key}={d.Value.GetHashCode()}");
                expressionCacheKey += $"?{String.Join("&", parameterStrings)}";
            }

            var hash = __hashFunction.ComputeHash(expressionCacheKey);

            return hash;
        }


    }
}

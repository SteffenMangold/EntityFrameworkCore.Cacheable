using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.Cacheable
{
    /// <summary>
    /// A cache provider to store and recieve query results./>
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// Creates a unique key to identify a query expression.
        /// </summary>
        /// <param name="expression">Query expression</param>
        /// <param name="parameterValues">Query parameter values</param>
        /// <returns>Unique key object</returns>
        object CreateQueryKey(Expression expression, IReadOnlyDictionary<string, object> parameterValues);

        /// <summary>
        /// Add a query result to cache storage, by given key.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key">Key to identify query result</param>
        /// <param name="value">Query result</param>
        /// <param name="cacheableResultOperator">Options</param>
        void SetCachedResult<TResult>(object key, TResult value, TimeSpan timeToLive);

        /// <summary>
        /// Try to get a cached query result from storage.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key">Key to identify query result</param>
        /// <param name="cacheResult">Stored query result if key was found</param>
        /// <returns>Returns <see cref="true"/> when the given key was found <see cref="false"/> otherwise.</returns>
        bool TryGetCachedResult<TResult>(object key, out TResult cacheResult);
    }
}
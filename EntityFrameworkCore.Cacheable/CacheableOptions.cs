using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Cacheable
{
    /// <summary>
    /// Options how to handle result caching.
    /// </summary>
    public class CacheableOptions
    {
        /// <summary>
        /// Limits the lifetime of cached query results. Default value is 5 minutes.
        /// </summary>
        public TimeSpan TimeToLive { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Set true if null result should be cached, otherwise false. Default value is true.
        /// </summary>
        public Boolean CacheNullResult { get; set; } = true;

        /// <summary>
        /// Set true if empty <see cref="IEnumerable{T}"/> result should be cached, otherwise false. Default value is true.
        /// </summary>
        /// <remarks>
        /// This options does not work in Async queries.
        /// </remarks>
        public Boolean CacheEmptyResult { get; set; } = true;
    }
}

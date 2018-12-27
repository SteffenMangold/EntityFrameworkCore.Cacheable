using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Cacheable.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Event IDs for events that correspond to messages logged to an <see cref="ILogger" />
    ///         and events sent to a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
    ///         behavior of warnings.
    ///     </para>
    /// </summary>
    public static class CacheableEventId
    {
        /// <summary>
        ///     The lower-bound for event IDs used by any Entity Framework or provider code.
        /// </summary>
        public const int CacheableBaseId = 50000;

        // Warning: These values must not change between releases.
        // Only add new values to the end of sections, never in the middle.
        // Try to use <Noun><Verb> naming and be consistent with existing names.
        private enum Id
        {
            // Query events
            CacheHit = CacheableBaseId,
            //RowLimitingOperationWithoutOrderByWarning,
            //FirstWithoutOrderByAndFilterWarning,

            //// Infrastructure events
            //SensitiveDataLoggingEnabledWarning = CacheableBaseId + 100,
            //ServiceProviderCreated,
        }

        private static readonly string _queryPrefix = DbLoggerCategory.Query.Name + ".";
        private static EventId MakeQueryId(Id id) => new EventId((int)id, _queryPrefix + id);

        /// <summary>
        ///     <para>
        ///         A query result is returned from cache.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId CacheHit = MakeQueryId(Id.CacheHit);
    }
}

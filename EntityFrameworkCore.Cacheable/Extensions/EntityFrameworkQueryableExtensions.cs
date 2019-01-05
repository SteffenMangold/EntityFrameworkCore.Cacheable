using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EntityFrameworkCore.Cacheable
{
    public static class EntityFrameworkQueryableExtensions
    {
        internal static readonly MethodInfo CacheableMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
            .GetTypeInfo()
            .GetMethods()
            .Where(m => m.Name == nameof(Cacheable))
            .Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(CacheableOptions)))
            .Single();

        /// <summary>
        /// Returns a new query where the result will be cached base on the <see cref="TimeSpan"/> parameter.
        /// </summary>
        /// <typeparam name="T">The type of entity being queried.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="timeToLive">Limits the lifetime of cached query results.</param>
        /// <returns>A new query where the result set will be cached.</returns>
        public static IQueryable<T> Cacheable<T>(this IQueryable<T> source, [NotParameterized] TimeSpan timeToLive)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(timeToLive, nameof(timeToLive));

            return source.Cacheable<T>(new CacheableOptions
            {
                TimeToLive = timeToLive
            });
        }

        /// <summary>
        /// Returns a new query where the result will be cached base on the <see cref="TimeSpan"/> parameter.
        /// </summary>
        /// <typeparam name="T">The type of entity being queried.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="options">Options how to handle cached query results.</param>
        /// <returns>A new query where the result set will be cached.</returns>
        public static IQueryable<T> Cacheable<T>(this IQueryable<T> source, [NotParameterized] CacheableOptions options)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(options, nameof(options));

            return
               source.Provider is EntityQueryProvider
                   ? source.Provider.CreateQuery<T>(
                       Expression.Call(
                           instance: null,
                           method: CacheableMethodInfo.MakeGenericMethod(typeof(T)),
                           arg0: source.Expression,
                           arg1: Expression.Constant(options)))
                   : source;
        }
    }
}

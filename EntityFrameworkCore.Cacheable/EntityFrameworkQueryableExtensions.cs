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
        internal static readonly MethodInfo CacheablehMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(Cacheable));

        public static IQueryable<T> Cacheable<T>(this IQueryable<T> source, [NotParameterized] TimeSpan timeToLive)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(timeToLive, nameof(timeToLive));

            return
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<T>(
                        Expression.Call(
                            instance: null,
                            method: CacheablehMethodInfo.MakeGenericMethod(typeof(T)),
                            arg0: source.Expression,
                            arg1: Expression.Constant(timeToLive)))
                    : source;
        }
    }
}

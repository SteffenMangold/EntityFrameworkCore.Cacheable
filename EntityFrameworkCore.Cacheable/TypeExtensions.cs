using System;
using System.Reflection;

namespace EntityFrameworkCore.Cacheable
{
    internal static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
            => !type.IsValueType
               || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));

        public static Type UnwrapEnumType(this Type type)
            => type.GetTypeInfo().IsEnum ? Enum.GetUnderlyingType(type) : type;

        public static Type UnwrapNullableType(this Type type)
            => Nullable.GetUnderlyingType(type) ?? type;
    }
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.Cacheable
{
    public class CacheableOptionsExtension : IDbContextOptionsExtension
    {
        ICacheProvider _cacheProvider;

        internal CacheableOptionsExtension(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public string LogFragment => $"Using {_cacheProvider.GetType().Name}";

        public bool ApplyServices(IServiceCollection services)
        {
            services.AddSingleton(_cacheProvider);

            return false;
        }

        public long GetServiceProviderHashCode() => 0L;

        public void Validate(IDbContextOptions options)
        {            
        }

        public void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["EntityFrameworkCore.Cacheable:" + nameof(_cacheProvider)] = _cacheProvider.GetType().ToString();
        }

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.UseSecondLevelMemoryCache" /> method.
        /// </summary>
        public virtual ICacheProvider CacheProvider => _cacheProvider;
    }
}
//"Method 'PopulateDebugInfo' in type 'EntityFrameworkCore.Cacheable.CacheableOptionsExtension' from assembly 'EntityFrameworkCore.Cacheable, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null' does not have an implementation."
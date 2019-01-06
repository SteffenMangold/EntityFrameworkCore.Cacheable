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
            services.AddSingleton<ICacheProvider>(_cacheProvider);

            return false;
        }

        public long GetServiceProviderHashCode() => 0L;

        public void Validate(IDbContextOptions options)
        {            
        }

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.UseSecondLevelMemoryCache" /> method.
        /// </summary>
        public virtual ICacheProvider CacheProvider => _cacheProvider;
    }
}

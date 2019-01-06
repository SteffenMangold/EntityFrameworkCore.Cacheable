using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace EntityFrameworkCore.Cacheable
{
    public static class DbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Configures the context to support second level query caching.
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder UseSecondLevelCache(this DbContextOptionsBuilder optionsBuilder)
        {
            return optionsBuilder.UseSecondLevelCache(new MemoryCacheProvider());
        }

        /// <summary>
        /// Configures the context to support second level query caching.
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="cacheProvider">The cache provider to storage query results.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder UseSecondLevelCache(this DbContextOptionsBuilder optionsBuilder, ICacheProvider cacheProvider)
        {
            optionsBuilder.ReplaceService<IQueryCompiler, CustomQueryCompiler>();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new CacheableOptionsExtension(cacheProvider));

            return optionsBuilder;
        }
    }
}

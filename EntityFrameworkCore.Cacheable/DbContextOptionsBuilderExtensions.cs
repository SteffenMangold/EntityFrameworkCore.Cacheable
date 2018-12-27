using Microsoft.EntityFrameworkCore;
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
        public static DbContextOptionsBuilder UseSecondLevelMemoryCache(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IQueryCompiler, CustomQueryCompiler>();

            return optionsBuilder;
        }
    }
}

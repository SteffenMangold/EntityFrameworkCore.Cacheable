using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace EntityFrameworkCore.Cacheable
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseSecondLevelMemoryCache(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IQueryCompiler, CustomQueryCompiler>();
            optionsBuilder.ReplaceService<INodeTypeProviderFactory, CustomMethodInfoBasedNodeTypeRegistryFactory>();

            return optionsBuilder;
        }
    }
}

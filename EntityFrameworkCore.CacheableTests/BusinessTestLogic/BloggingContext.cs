using EntityFrameworkCore.Cacheable;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.CacheableTests.BusinessTestLogic
{
    public class BloggingContext : DbContext
    {
        public BloggingContext()
        { }

        public BloggingContext(DbContextOptions<BloggingContext> options)
            : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSecondLevelCache();
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
    }
}

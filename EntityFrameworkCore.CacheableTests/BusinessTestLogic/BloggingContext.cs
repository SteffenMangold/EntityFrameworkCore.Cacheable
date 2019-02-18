using EntityFrameworkCore.Cacheable;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.CacheableTests.BusinessTestLogic
{
    public class BloggingContext : DbContext
    {
        private readonly int? _minBlogId;

        public BloggingContext()
        { }

        public BloggingContext(DbContextOptions<BloggingContext> options, int? minBlogId = null)
            : base(options)
        {
            _minBlogId = minBlogId;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSecondLevelCache();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add global query filters to all entities
            if (_minBlogId.HasValue)
            {
                modelBuilder.Entity<Blog>()
                       .HasQueryFilter(e => e.BlogId >= _minBlogId); 
            }
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
    }
}

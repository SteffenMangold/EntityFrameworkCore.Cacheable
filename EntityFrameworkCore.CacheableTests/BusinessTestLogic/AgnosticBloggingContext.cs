using EntityFrameworkCore.Cacheable;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.CacheableTests.BusinessTestLogic
{
    public class AgnosticBloggingContext : DbContext
    {
        private readonly int? _minBlogId;

        public AgnosticBloggingContext()
        { }

        public AgnosticBloggingContext(DbContextOptions<AgnosticBloggingContext> options, int? minBlogId = null)
            : base(options)
        {
            _minBlogId = minBlogId;
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

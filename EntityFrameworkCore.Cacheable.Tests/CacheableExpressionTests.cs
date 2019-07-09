using EntityFrameworkCore.Cacheable.Diagnostics;
using EntityFrameworkCore.CacheableTests.BusinessTestLogic;
using EntityFrameworkCore.CacheableTests.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xunit;

namespace EntityFrameworkCore.Cacheable.Tests
{
    [Trait("Category", "EntityFrameworkCore.Cacheable.Expressions")]
    public class CacheableExpressionTests
    {
        /// <summary>
        /// Testing cache expiration functionality.
        /// </summary>
        //[Fact]
        public void ExpirationTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<BloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "ExpirationTest")
                .Options;

            // create test entries
            using (var initContext = new BloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var expirationContext = new BloggingContext(options))
            {
                // shoud not hit cache, because first execution
                var result = expirationContext.Blogs
                    .Where(d => d.BlogId == 1)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                // shoud hit cache, because second execution
                result = expirationContext.Blogs
                    .Where(d => d.BlogId == 1)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                // shoud not hit cache, because different parameter
                result = expirationContext.Blogs
                    .Where(d => d.BlogId == 2)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                Thread.Sleep(TimeSpan.FromSeconds(10));

                // shoud not hit cache, because expiration
                result = expirationContext.Blogs
                    .Where(d => d.BlogId == 1)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.True(logs.Count() == 1);
        }

        /// <summary>
        /// Testing entity result cache functionality.
        /// </summary>
        [Fact]
        public void EntityExpressionTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<BloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "EntityExpressionTest")
                .Options;

            // create test entries
            using (var initContext = new BloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var entityContext = new BloggingContext(options))
            {
                // shoud not hit cache, because first execution
                var result = entityContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToList();

                // shoud hit cache, because second execution
                var cachedResult = entityContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToList();

                Assert.Equal(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.True(logs.Count() == 1);
        }

        /// <summary>
        /// Testing projection result cache functionality.
        /// </summary>
        [Fact]
        public void ProjectionExpressionTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<BloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "ProjectionExpressionTest")
                .Options;

            // create test entries
            using (var initContext = new BloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var projectionContext = new BloggingContext(options))
            {
                // shoud not hit cache, because first execution
                var result = projectionContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Select(d => new
                    {
                        d.BlogId,
                        d.Rating
                    })
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToList();

                // shoud hit cache, because second execution
                var cachedResult = projectionContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Select(d => new
                    {
                        d.BlogId,
                        d.Rating
                    })
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToList();

                Assert.Equal(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.True(logs.Count() == 1);
        }

        /// <summary>
        /// Testing projection result cache functionality.
        /// </summary>
        [Fact]
        public void SingleProjectionExpressionTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<BloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "ProjectionExpressionTest")
                .Options;

            // create test entries
            using (var initContext = new BloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var projectionContext = new BloggingContext(options))
            {
                // shoud not hit cache, because first execution
                var result = projectionContext.Blogs
                    .Where(d => d.BlogId == 1)
                    .Select(d => new
                    {
                        d.BlogId,
                        d.Rating
                    })
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .SingleOrDefault();

                Thread.Sleep(TimeSpan.FromSeconds(1));

                // shoud hit cache, because second execution
                var cachedResult = projectionContext.Blogs
                    .Where(d => d.BlogId == 1)
                    .Select(d => new
                    {
                        d.BlogId,
                        d.Rating
                    })
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .SingleOrDefault();

                Assert.NotNull(result);
                Assert.Same(result, cachedResult);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.True(logs.Count() == 1);
        }

        /// <summary>
        /// Testing constant result cache functionality.
        /// </summary>
        [Fact]
        public void ConstantExpressionTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<BloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "ConstantExpressionTest")
                .Options;

            // create test entries
            using (var initContext = new BloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var constantContext = new BloggingContext(options))
            {
                // shoud not hit cache, because first execution
                var result = constantContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Select(d => d.BlogId)
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToList();

                // shoud hit cache, because second execution
                var cachedResult = constantContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Select(d => d.BlogId)
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToList();

                Assert.Equal(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.True(logs.Count() == 1);
        }

        /// <summary>
        /// Testing null parameter query .
        /// </summary>
        [Fact]
        public void NullValueExpressionTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<BloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "ConstantExpressionTest")
                .Options;

            // create test entries
            using (var initContext = new BloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            int? ratingValue = null;

            using (var constantContext = new BloggingContext(options))
            {
                // shoud not hit cache, because first execution
                var result = constantContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Where(d => d.Rating == ratingValue)
                    .Select(d => d.BlogId)
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToList();

                // shoud hit cache, because second execution
                var cachedResult = constantContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Where(d => d.Rating == ratingValue)
                    .Select(d => d.BlogId)
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToList();

                Assert.Equal(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.True(logs.Count() == 1);
        }

        /// <summary>
        /// Testing null parameter query .
        /// </summary>
        [Fact]
        public void GlobalQueryFilterTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<BloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "GlobalQueryFilterTest")
                .Options;

            // create test entries
            using (var initContext = new BloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var constantContext = new BloggingContext(options, minBlogId: 2))
            {
                // shoud not hit cache, because no Cacheable call
                var rawResult = constantContext.Blogs
                    .Count();

                // shoud not hit cache, because first execution
                var result = constantContext.Blogs
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .Count();

                // shoud hit cache, because second execution
                var cachedResult = constantContext.Blogs
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .Count();

                Assert.Equal(result, cachedResult);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.True(logs.Count() == 1);
        }

        /// <summary>
        /// Testing performance of cache functionality.
        /// </summary>
        /// <remarks>
        /// It only tests agains InMemory database, so the test is not expected to be much faster.
        /// </remarks>
        [Fact]
        public void PerformanceTest()
        {
            MemoryCacheProvider.ClearCache();

            decimal loopCount = 1000;

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<BloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "PerformanceTest")
                .Options;

            // create test entries
            using (var initContext = new BloggingContext(options))
            {
                initContext.ChangeTracker.AutoDetectChangesEnabled = false;

                for (int i = 0; i < 100000; i++)
                {
                    initContext.Blogs.Add(new Blog
                    {
                        Url = $"http://sample.com/cat{i}",
                        
                        Posts = new List<Post>
                        {
                            { new Post {Title = $"Post{1}"} }
                        }
                    }); 
                }
                initContext.SaveChanges();
            }

            var rawOptions = new DbContextOptionsBuilder<BloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "PerformanceTest")
                .Options;

            using (var performanceContext = new BloggingContext(rawOptions))
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                // raw queries
                for (int i = 0; i < loopCount; i++)
                {
                    var result = performanceContext.Blogs
                        .Where(d => d.BlogId >= 0)
                        .Take(100)
                        .ToList();
                }

                var rawTimeSpan = watch.Elapsed;

                Debug.WriteLine($"Average default context database query duration [+{TimeSpan.FromTicks((long)(rawTimeSpan.Ticks / loopCount))}].");
            }

            using (var performanceContext = new BloggingContext(options))
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                // uncached queries
                for (int i = 0; i < loopCount; i++)
                {
                    var result = performanceContext.Blogs
                        .Where(d => d.BlogId >= 0)
                        .Take(100)
                        .ToList();
                }

                var uncachedTimeSpan = watch.Elapsed;

                // caching query result
                performanceContext.Blogs
                    .Where(d => d.BlogId >= 0)
                    .Cacheable(TimeSpan.FromMinutes(10))
                    .Take(100)
                    .ToList();

                watch.Restart();

                // cached queries
                for (int i = 0; i < loopCount; i++)
                {
                    var result = performanceContext.Blogs
                        .Where(d => d.BlogId >= 0)
                        .Cacheable(TimeSpan.FromMinutes(10))
                        .Take(100)
                        .ToList();
                }

                var cachedTimeSpan = watch.Elapsed;


                // find log entries
                var queryResultsCachedCount = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.QueryResultCached).Count();
                var cacheHitsCount = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit).Count();

                // check cache event counts
                Assert.True(queryResultsCachedCount == 1);
                Assert.True(cacheHitsCount == loopCount);

                Debug.WriteLine($"Average database query duration [+{TimeSpan.FromTicks((long)(uncachedTimeSpan.Ticks / loopCount))}].");
                Debug.WriteLine($"Average cache query duration [+{TimeSpan.FromTicks((long)(cachedTimeSpan.Ticks / loopCount))}].");
                Debug.WriteLine($"Cached queries are x{((Decimal)uncachedTimeSpan.Ticks / (Decimal)cachedTimeSpan.Ticks)-1:N2} times faster.");

                Assert.True(cachedTimeSpan < uncachedTimeSpan);
            }
        }
    }
}
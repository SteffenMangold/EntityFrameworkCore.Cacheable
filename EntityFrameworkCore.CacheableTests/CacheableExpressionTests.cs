using EntityFrameworkCore.Cacheable.Diagnostics;
using EntityFrameworkCore.CacheableTests;
using EntityFrameworkCore.CacheableTests.BusinessTestLogic;
using EntityFrameworkCore.CacheableTests.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EntityFrameworkCore.Cacheable.Tests
{
    [TestClass]
    [TestCategory("EntityFrameworkCore.Cacheable")]
    public class CustomQueryCompilerTests
    {
        /// <summary>
        /// Testing cache expiration functionality.
        /// </summary>
        //[TestMethod]
        public void ExpirationTest()
        {
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
            Assert.IsTrue(logs.Count() == 1);
        }

        /// <summary>
        /// Testing entity result cache functionality.
        /// </summary>
        [TestMethod]
        public void EntityExpressionTest()
        {
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
                    .Where(d => d.BlogId == 2)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                // shoud hit cache, because second execution
                var cachedResult = entityContext.Blogs
                    .Where(d => d.BlogId == 2)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                Assert.AreEqual(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.IsTrue(logs.Count() == 1);
        }

        /// <summary>
        /// Testing projection result cache functionality.
        /// </summary>
        [TestMethod]
        public void ProjectionExpressionTest()
        {
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
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                // shoud hit cache, because second execution
                var cachedResult = projectionContext.Blogs
                    .Where(d => d.BlogId == 1)
                    .Select(d => new
                    {
                        d.BlogId,
                        d.Rating
                    })
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                Assert.AreEqual(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.IsTrue(logs.Count() == 1);
        }

        /// <summary>
        /// Testing constant result cache functionality.
        /// </summary>
        [TestMethod]
        public void ConstantExpressionTest()
        {
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
                    .Where(d => d.BlogId == 1)
                    .Select(d => d.BlogId)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                // shoud hit cache, because second execution
                var cachedResult = constantContext.Blogs
                    .Where(d => d.BlogId == 1)
                    .Select(d => d.BlogId)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                Assert.AreEqual(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.IsTrue(logs.Count() == 1);
        }
        
        /// <summary>
        /// Testing performance of cache functionality.
        /// </summary>
        /// <remarks>
        /// It only tests agains InMemory database, so the test is not expected to be much faster.
        /// </remarks>
        [TestMethod]
        public void PerformanceTest()
        {
            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<BloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "PerformanceTest")
                .Options;

            // create test entries
            using (var initContext = new BloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var performanceContext = new BloggingContext(options))
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                // uncached queries
                for (int i = 0; i < 20; i++)
                {
                    performanceContext.Blogs
                        .Where(d => d.BlogId == 1)
                        .ToList();
                }

                var uncachedTimeSpan = watch.Elapsed;
                watch.Restart();

                // cached queries
                for (int i = 0; i < 20; i++)
                {
                    performanceContext.Blogs
                        .Where(d => d.BlogId == 1)
                        .Cacheable(TimeSpan.FromMinutes(10))
                        .ToList();
                }

                var cachedTimeSpan = watch.Elapsed;


                Debug.WriteLine($"Cache is x{Math.Round((Decimal)uncachedTimeSpan.Ticks / (Decimal)cachedTimeSpan.Ticks, 1)} times faster.");

                Assert.IsTrue(cachedTimeSpan < uncachedTimeSpan);
            }
        }
    }
}
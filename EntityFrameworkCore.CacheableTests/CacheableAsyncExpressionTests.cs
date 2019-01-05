using EntityFrameworkCore.Cacheable.Diagnostics;
using EntityFrameworkCore.CacheableTests;
using EntityFrameworkCore.CacheableTests.BusinessTestLogic;
using EntityFrameworkCore.CacheableTests.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Cacheable.Tests
{
    [TestClass]
    [TestCategory("EntityFrameworkCore.Cacheable.AsyncExpressions")]
    public class CacheableAsyncExpressionTests
    {
        /// <summary>
        /// Testing entity result cache functionality.
        /// </summary>
        [TestMethod]
        public async Task EntityAsyncExpressionTest()
        {
            CacheProvider.ClearCache();

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
                var result = await entityContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToListAsync();

                // shoud hit cache, because second execution
                var cachedResult = await entityContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToListAsync();

                Assert.AreEqual(2, result.Count);
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
        public async Task ProjectionAsyncExpressionTest()
        {
            CacheProvider.ClearCache();

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
                var result = await projectionContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Select(d => new
                    {
                        d.BlogId,
                        d.Rating
                    })
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToListAsync();

                // shoud hit cache, because second execution
                var cachedResult = await projectionContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Select(d => new
                    {
                        d.BlogId,
                        d.Rating
                    })
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToListAsync();

                Assert.AreEqual(2, result.Count);
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
        public async Task SingleProjectionAsyncExpressionTest()
        {
            CacheProvider.ClearCache();

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
                var result = await projectionContext.Blogs
                    .Where(d => d.BlogId == 1)
                    .Select(d => new
                    {
                        d.BlogId,
                        d.Rating
                    })
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .SingleOrDefaultAsync();

                // shoud hit cache, because second execution
                var cachedResult = await projectionContext.Blogs
                    .Where(d => d.BlogId == 1)
                    .Select(d => new
                    {
                        d.BlogId,
                        d.Rating
                    })
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .SingleOrDefaultAsync();

                Assert.IsNotNull(result);
                Assert.AreSame(result, cachedResult);
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
        public async Task ConstantAsyncExpressionTest()
        {
            CacheProvider.ClearCache();

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
                var result = await constantContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Select(d => d.BlogId)
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToListAsync();

                // shoud hit cache, because second execution
                var cachedResult = await constantContext.Blogs
                    .Where(d => d.BlogId > 1)
                    .Select(d => d.BlogId)
                    .Cacheable(TimeSpan.FromMinutes(5))
                    .ToListAsync();

                Assert.AreEqual(2, result.Count);
                Assert.AreEqual(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.IsTrue(logs.Count() == 1);
        }
    }
}
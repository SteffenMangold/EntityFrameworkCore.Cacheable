﻿using EntityFrameworkCore.Cacheable.Diagnostics;
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

namespace EntityFrameworkCore.Cacheable.Tests
{
    [TestClass]
    [TestCategory("EntityFrameworkCore.Cacheable.Expressions")]
    public class CacheableAgnosticContextTests
    {
        /// <summary>
        /// Testing cache expiration functionality.
        /// </summary>
        //[TestMethod]
        public void ExpirationTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<AgnosticBloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "ExpirationTest")
                .UseSecondLevelCache()
                .Options;

            // create test entries
            using (var initContext = new AgnosticBloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var expirationContext = new AgnosticBloggingContext(options))
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
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<AgnosticBloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "EntityExpressionTest")
                .UseSecondLevelCache()
                .Options;

            // create test entries
            using (var initContext = new AgnosticBloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var entityContext = new AgnosticBloggingContext(options))
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
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<AgnosticBloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "ProjectionExpressionTest")
                .UseSecondLevelCache()
                .Options;

            // create test entries
            using (var initContext = new AgnosticBloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var projectionContext = new AgnosticBloggingContext(options))
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
        public void SingleProjectionExpressionTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<AgnosticBloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "ProjectionExpressionTest")
                .UseSecondLevelCache()
                .Options;

            // create test entries
            using (var initContext = new AgnosticBloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var projectionContext = new AgnosticBloggingContext(options))
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
        public void ConstantExpressionTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<AgnosticBloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "ConstantExpressionTest")
                .UseSecondLevelCache()
                .Options;

            // create test entries
            using (var initContext = new AgnosticBloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var constantContext = new AgnosticBloggingContext(options))
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

                Assert.AreEqual(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.IsTrue(logs.Count() == 1);
        }

        /// <summary>
        /// Testing null parameter query .
        /// </summary>
        [TestMethod]
        public void NullValueExpressionTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<AgnosticBloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "ConstantExpressionTest")
                .UseSecondLevelCache()
                .Options;

            // create test entries
            using (var initContext = new AgnosticBloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            int? ratingValue = null;

            using (var constantContext = new AgnosticBloggingContext(options))
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

                Assert.AreEqual(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = loggerProvider.Entries.Where(e => e.EventId == CacheableEventId.CacheHit);

            // cache should hit one time
            Assert.IsTrue(logs.Count() == 1);
        }

        /// <summary>
        /// Testing null parameter query .
        /// </summary>
        [TestMethod]
        public void GlobalQueryFilterTest()
        {
            MemoryCacheProvider.ClearCache();

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<AgnosticBloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "GlobalQueryFilterTest")
                .UseSecondLevelCache()
                .Options;

            // create test entries
            using (var initContext = new AgnosticBloggingContext(options))
            {
                initContext.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                initContext.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                initContext.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                initContext.SaveChanges();
            }

            using (var constantContext = new AgnosticBloggingContext(options, minBlogId: 2))
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

                Assert.AreEqual(result, cachedResult);
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
            MemoryCacheProvider.ClearCache();

            decimal loopCount = 1000;

            var loggerProvider = new DebugLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            var options = new DbContextOptionsBuilder<AgnosticBloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "PerformanceTest")
                .UseSecondLevelCache()
                .Options;

            // create test entries
            using (var initContext = new AgnosticBloggingContext(options))
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

            var rawOptions = new DbContextOptionsBuilder<AgnosticBloggingContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase(databaseName: "PerformanceTest")
                .Options;

            using (var performanceContext = new AgnosticBloggingContext(rawOptions))
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

            using (var performanceContext = new AgnosticBloggingContext(options))
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
                Assert.IsTrue(queryResultsCachedCount == 1);
                Assert.IsTrue(cacheHitsCount == loopCount);

                Debug.WriteLine($"Average database query duration [+{TimeSpan.FromTicks((long)(uncachedTimeSpan.Ticks / loopCount))}].");
                Debug.WriteLine($"Average cache query duration [+{TimeSpan.FromTicks((long)(cachedTimeSpan.Ticks / loopCount))}].");
                Debug.WriteLine($"Cached queries are x{((Decimal)uncachedTimeSpan.Ticks / (Decimal)cachedTimeSpan.Ticks) - 1:N2} times faster.");

                Assert.IsTrue(cachedTimeSpan < uncachedTimeSpan);
            }
        }
    }
}
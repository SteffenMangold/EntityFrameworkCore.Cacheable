using EntityFrameworkCore.CacheableTests;
using EntityFrameworkCore.CacheableTests.BusinessTestLogic;
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
        private static DbContextOptions<BloggingContext> CreateDatabaseOption(LoggerFactory debugLoggerFactory)
        {
            // use InMemory Database for testing
            return new DbContextOptionsBuilder<BloggingContext>()
                .UseInMemoryDatabase(databaseName: "BusinessTestLogicDB")
                .UseLoggerFactory(debugLoggerFactory)
                .Options;
        }

        [ClassInitialize]
        public static void HandlerTestsInit(TestContext testContext)
        {
            // create logger to detect cache hits
            var debugLoggerProvider = new DebugLoggerProvider();
            
            // create test entries
            using (var context = new BloggingContext(CreateDatabaseOption(new LoggerFactory(new[] { debugLoggerProvider }))))
            {
                context.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                context.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                context.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                context.SaveChanges();
            }            
        }

        /// <summary>
        /// Testing cache expiration functionality.
        /// </summary>
        [TestMethod]
        public void ExpirationTest()
        {
            // create logger to detect cache hits
            var debugLoggerProvider = new DebugLoggerProvider();

            using (var context = new BloggingContext(CreateDatabaseOption(new LoggerFactory(new[] { debugLoggerProvider }))))
            {
                // shoud not hit cache, because first execution
                var result = context.Blogs
                    .Where(d => d.BlogId == 1)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                // shoud hit cache, because second execution
                result = context.Blogs
                    .Where(d => d.BlogId == 1)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                // shoud not hit cache, because different parameter
                result = context.Blogs
                    .Where(d => d.BlogId == 2)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                Thread.Sleep(TimeSpan.FromSeconds(10));

                // shoud not hit cache, because expiration
                result = context.Blogs
                    .Where(d => d.BlogId == 1)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();
            }

            // find "cache hit" log entries
            var logs = debugLoggerProvider.FindLogEnties(100199);

            // cache should hit one time
            Assert.IsTrue(logs.Length == 1);
        }

        /// <summary>
        /// Testing entity result cache functionality.
        /// </summary>
        [TestMethod]
        public void EntityExpressionTest()
        {
            // create logger to detect cache hits
            var debugLoggerProvider = new DebugLoggerProvider();

            using (var context = new BloggingContext(CreateDatabaseOption(new LoggerFactory(new[] { debugLoggerProvider }))))
            {
                // shoud not hit cache, because first execution
                var result = context.Blogs
                    .Where(d => d.BlogId == 1)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                // shoud hit cache, because second execution
                var cachedResult = context.Blogs
                    .Where(d => d.BlogId == 1)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                Assert.AreEqual(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = debugLoggerProvider.FindLogEnties(100199);

            // cache should hit one time
            Assert.IsTrue(logs.Length == 1);
        }

        /// <summary>
        /// Testing projection result cache functionality.
        /// </summary>
        [TestMethod]
        public void ProjectionExpressionTest()
        {
            // create logger to detect cache hits
            var debugLoggerProvider = new DebugLoggerProvider();

            using (var context = new BloggingContext(CreateDatabaseOption(new LoggerFactory(new[] { debugLoggerProvider }))))
            {
                // shoud not hit cache, because first execution
                var result = context.Blogs
                    .Where(d => d.BlogId == 1)
                    .Select(d => new
                    {
                        d.BlogId,
                        d.Rating
                    })
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                // shoud hit cache, because second execution
                var cachedResult = context.Blogs
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
            var logs = debugLoggerProvider.FindLogEnties(100199);

            // cache should hit one time
            Assert.IsTrue(logs.Length == 1);
        }

        /// <summary>
        /// Testing constant result cache functionality.
        /// </summary>
        [TestMethod]
        public void ConstantExpressionTest()
        {
            // create logger to detect cache hits
            var debugLoggerProvider = new DebugLoggerProvider();

            using (var context = new BloggingContext(CreateDatabaseOption(new LoggerFactory(new[] { debugLoggerProvider }))))
            {
                // shoud not hit cache, because first execution
                var result = context.Blogs
                    .Where(d => d.BlogId == 1)
                    .Select(d => d.BlogId)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                // shoud hit cache, because second execution
                var cachedResult = context.Blogs
                    .Where(d => d.BlogId == 1)
                    .Select(d => d.BlogId)
                    .Cacheable(TimeSpan.FromSeconds(5))
                    .ToList();

                Assert.AreEqual(result.Count, cachedResult.Count);
            }

            // find "cache hit" log entries
            var logs = debugLoggerProvider.FindLogEnties(100199);

            // cache should hit one time
            Assert.IsTrue(logs.Length == 1);
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
            // create logger to detect cache hits
            var debugLoggerProvider = new DebugLoggerProvider();

            using (var context = new BloggingContext(CreateDatabaseOption(new LoggerFactory(new[] { debugLoggerProvider }))))
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                // uncached queries
                for (int i = 0; i < 20; i++)
                {
                    context.Blogs
                        .Where(d => d.BlogId == 1)
                        .ToList();
                }

                var uncachedTimeSpan = watch.Elapsed;
                watch.Restart();

                // cached queries
                for (int i = 0; i < 20; i++)
                {
                    context.Blogs
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
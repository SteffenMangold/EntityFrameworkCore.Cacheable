using EntityFrameworkCore.CacheableTests;
using EntityFrameworkCore.CacheableTests.BusinessTestLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;

namespace EntityFrameworkCore.Cacheable.Tests
{
    [TestClass]
    [TestCategory("EntityFrameworkCore.Cacheable")]
    public class CustomQueryCompilerTests
    {
        private static DbContextOptions<BloggingContext> _options;
        private static DebugLoggerProvider _debugLoggerProvider;
        private static LoggerFactory _debugLoggerFactory;

        [ClassInitialize]
        public static void HandlerTestsInit(TestContext testContext)
        {
            // create logger to detect cache hits
            _debugLoggerProvider = new DebugLoggerProvider();
            _debugLoggerFactory = new LoggerFactory(new[] { _debugLoggerProvider });

            // use InMemory Database for testing
            _options = new DbContextOptionsBuilder<BloggingContext>()
                .UseInMemoryDatabase(databaseName: "BusinessTestLogicDB")    
                .UseLoggerFactory(_debugLoggerFactory)
                .Options;

            // create test entries
            using (var context = new BloggingContext(_options))
            {
                context.Blogs.Add(new Blog { BlogId = 1, Url = "http://sample.com/cats" });
                context.Blogs.Add(new Blog { BlogId = 2, Url = "http://sample.com/catfish" });
                context.Blogs.Add(new Blog { BlogId = 3, Url = "http://sample.com/dogs" });
                context.SaveChanges();
            }            
        }

        /// <summary>
        /// Testing general cache functionality.
        /// </summary>
        [TestMethod]
        public void CacheableExpression()
        {           
            using (var context = new BloggingContext(_options))
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
            var logs = _debugLoggerProvider.FindLogEnties(100199);

            // cache should hit one time
            Assert.IsTrue(logs.Length == 1);
        }
    }
}
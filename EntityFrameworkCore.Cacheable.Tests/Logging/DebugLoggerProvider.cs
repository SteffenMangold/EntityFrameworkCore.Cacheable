using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EntityFrameworkCore.CacheableTests.Logging
{
    public sealed class DebugLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, DebugLogger> _loggers;
        private readonly ConcurrentBag<LogMessageEntry> _messageQueue = new ConcurrentBag<LogMessageEntry>();

        public IReadOnlyCollection<LogMessageEntry> Entries => _messageQueue;

        public DebugLoggerProvider()
        {
            _loggers = new ConcurrentDictionary<string, DebugLogger>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, loggerName => new DebugLogger(_messageQueue));
        }

        public void Dispose()
        {
        }
    }
}

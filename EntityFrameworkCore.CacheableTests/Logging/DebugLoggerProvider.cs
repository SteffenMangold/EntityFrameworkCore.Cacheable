using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.CacheableTests.Logging
{
    public sealed class DebugLoggerProvider : ILoggerProvider
    {
        private const int _maxQueuedMessages = 1024;

        private readonly ConcurrentDictionary<string, DebugLogger> _loggers;
        private readonly BlockingCollection<LogMessageEntry> _messageQueue = new BlockingCollection<LogMessageEntry>(_maxQueuedMessages);

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
            _messageQueue.Dispose();
        }
    }
}

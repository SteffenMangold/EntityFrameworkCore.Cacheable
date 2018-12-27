using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.CacheableTests.Logging
{
    public class DebugLogger : ILogger
    {
        private const int _maxQueuedMessages = 1024;

        private readonly BlockingCollection<LogMessageEntry> _messageQueue;
                
        public DebugLogger(BlockingCollection<LogMessageEntry> messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Scope<TState>();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var formatedValue = formatter(state, exception);

            _messageQueue.Add(new LogMessageEntry(logLevel, eventId, formatedValue, exception));
        }

        class Scope<TState> : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace EntityFrameworkCore.CacheableTests.Logging
{
    public class DebugLogger : ILogger
    {
        private const int _maxQueuedMessages = 1024;

        private readonly ConcurrentBag<LogMessageEntry> _messageQueue;
                
        public DebugLogger(ConcurrentBag<LogMessageEntry> messageQueue)
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
            var entry =new LogMessageEntry(logLevel, eventId, formatedValue, exception);

            //var result = JsonConvert.SerializeObject(entry, Formatting.Indented);
            //Debug.WriteLine(result);

            _messageQueue.Add(entry);
        }

        class Scope<TState> : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}

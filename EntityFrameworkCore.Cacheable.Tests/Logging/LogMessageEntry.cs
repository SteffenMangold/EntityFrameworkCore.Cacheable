using Microsoft.Extensions.Logging;
using System;

namespace EntityFrameworkCore.CacheableTests.Logging
{
    public struct LogMessageEntry
    {
        public LogMessageEntry(LogLevel logLevel
            , EventId eventId
            , string message
            , Exception exception)
        {
            TimeStamp = DateTime.Now;
            LogLevel = logLevel;
            EventId = eventId;
            Message = message;
            Exception = exception;
        }

        public DateTime TimeStamp;
        public LogLevel LogLevel;
        public EventId EventId;
        public String Message;
        public Exception Exception;
    }
}

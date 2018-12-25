using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.CacheableTests
{
    public class DebugLogger : ILogger
    {
        List<Tuple<int, string>> _entries = new List<Tuple<int, string>>();

        public List<Tuple<int, string>> Entries => _entries;
        

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
            _entries.Add(new Tuple<int, string>(eventId.Id, formatedValue));
        }

        class Scope<TState> : IDisposable
        {


            public void Dispose()
            {
            }
        }
    }
}

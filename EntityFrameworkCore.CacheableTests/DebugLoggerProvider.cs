using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.CacheableTests
{
    public class DebugLoggerProvider : ILoggerProvider
    {
        Dictionary<String, DebugLogger> _loggers = new Dictionary<string, DebugLogger>();

        public ILogger CreateLogger(string categoryName)
        {
            if (!_loggers.ContainsKey(categoryName))
                _loggers.Add(categoryName, new DebugLogger());

            return _loggers[categoryName];
        }

        public String[] FindLogEnties(int id)
        {
            return _loggers.SelectMany(l => l.Value.Entries.Where(e => e.Item1 == id).Select(e => e.Item2)).ToArray();
        }

        public void Clear()
        {
            foreach (var logger in _loggers)
            {
                logger.Value.Entries.Clear();
            }
        }

        public void Dispose()
        {
            _loggers = null;
        }
    }
}

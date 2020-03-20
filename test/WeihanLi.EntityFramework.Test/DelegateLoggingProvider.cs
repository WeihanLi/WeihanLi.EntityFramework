using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace WeihanLi.EntityFramework.Test
{
    internal class DelegateLoggerProvider : ILoggerProvider
    {
        private readonly Action<string, LogLevel, Exception, string> _logAction;

        public DelegateLoggerProvider(Action<string, LogLevel, Exception, string> logAction)
        {
            _logAction = logAction;
        }

        private ConcurrentDictionary<string, DelegateLogger> _loggers = new ConcurrentDictionary<string, DelegateLogger>();

        public void Dispose()
        {
            _loggers.Clear();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, category => new DelegateLogger(category, _logAction));
        }

        private class DelegateLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly Action<string, LogLevel, Exception, string> _logAction;

            public DelegateLogger(string categoryName, Action<string, LogLevel, Exception, string> logAction)
            {
                _categoryName = categoryName;
                _logAction = logAction;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (null != _logAction)
                {
                    var msg = formatter(state, exception);
                    _logAction.Invoke(_categoryName, logLevel, exception, msg);
                }
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}

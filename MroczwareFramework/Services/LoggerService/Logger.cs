using log4net;
using log4net.Core;
using MroczwareFramework.Attributes.DependencyInjection;

namespace MroczwareFramework.Services.LoggerService
{
    [DependencyInjection(typeof(ILogger))]
    public class Logger : ILogger
    {
        private log4net.Core.ILogger _logger;
        public Logger() => _logger = LogManager.GetLogger(GetType()).Logger;

        public void Log(Type type, Level logType, string message, Exception ex)
        {
            _logger.Log(type, logType, message, ex);
        }
    }
}

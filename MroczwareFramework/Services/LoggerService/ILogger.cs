using log4net.Core;

namespace MroczwareFramework.Services.LoggerService
{
    public interface ILogger
    {
        void Log(Type type, Level logType, string message, Exception ex);
    }
}

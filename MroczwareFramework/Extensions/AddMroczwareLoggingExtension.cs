using Microsoft.Extensions.DependencyInjection;
using MroczwareFramework.Services.LoggerService;
using System.Reflection;
using System.Xml;
using ILogger = MroczwareFramework.Services.LoggerService.ILogger;

namespace MroczwareFramework.Extensions
{
    /// <summary>
    /// Rejestracja logowania bładów
    /// </summary>
    public static class AddMroczwareLoggingExtension
    {
        public static IServiceCollection AddMroczwareLogging(this IServiceCollection services)
        {
            var log4netConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");

            if (!File.Exists(log4netConfigPath))
                throw new FileNotFoundException($"Plik log4net.config nie został znaleziony w: {log4netConfigPath}");

            services.AddSingleton<ILogger, Logger>();

            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead(log4netConfigPath));
            var repo = log4net.LogManager.CreateRepository(Assembly.GetCallingAssembly(),
                       typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);

            return services;
        }
    }
}

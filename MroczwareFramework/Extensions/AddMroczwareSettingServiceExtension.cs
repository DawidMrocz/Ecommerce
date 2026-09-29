using Microsoft.Extensions.DependencyInjection;
using MroczwareFramework.Data;
using MroczwareFramework.Services.SettingService;

namespace MroczwareFramework.Extensions
{
    public static class AddMroczwareSettingServiceExtension
    {
        public static IServiceCollection AddMroczwareSettingService<TDbContext>(this IServiceCollection services) where TDbContext : FrameworkDbContext
        {
            services.AddScoped<ISettingService, SettingService<TDbContext>>();
            return services;
        }
    }
}

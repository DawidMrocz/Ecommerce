using Microsoft.Extensions.DependencyInjection;
using MroczwareFramework.Services.FileService;

namespace MroczwareFramework.Extensions
{
    /// <summary>
    /// Serwis do dodawania pliku lokalnie
    /// </summary>
    public static class AddFileServiceExtensionExtension
    {
        public static IServiceCollection AddMroczwareFileService<TDbContext>(this IServiceCollection services)
        {
            services.AddScoped<IFileService, FileService>();
            return services;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using MroczwareFramework.Data;
using MroczwareFramework.Services.EmailService;
using MroczwareFramework.Services.TemplateService;

namespace MroczwareFramework.Extensions
{
    /// <summary>
    /// Serwis do wysyłanie e-maila
    /// </summary>
    public static class AddMroczwareEmailServiceExtension
    {
        public static IServiceCollection AddMroczwareEmailService<TDbContext>(this IServiceCollection services) where TDbContext : FrameworkDbContext
        {
            services.AddScoped<ITemplateService, TemplateService<TDbContext>>();
            services.AddScoped<IEmailService, EmailService<TDbContext>>();
            return services;
        }
    }
}

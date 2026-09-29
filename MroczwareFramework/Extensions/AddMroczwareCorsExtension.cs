using Microsoft.Extensions.DependencyInjection;

namespace MroczwareFramework.Extensions
{
    public static class AddMroczwareCorsExtension
    {
        public static IServiceCollection AddMroczwareCors(this IServiceCollection services, string policyName, string origin)
        {
            services.AddCors(options =>
             {
                 options.AddPolicy(policyName, policy =>
                 {
                     policy.WithOrigins(origin)
                           .AllowCredentials()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                 });
             });

            return services;
        }
    }
}

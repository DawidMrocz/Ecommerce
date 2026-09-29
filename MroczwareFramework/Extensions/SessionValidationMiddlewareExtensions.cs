using Microsoft.AspNetCore.Builder;
using MroczwareFramework.Data;
using MroczwareFramework.Middleware;

namespace MroczwareFramework.Extensions
{
    public static class SessionValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionValidation<TDbContext>(this IApplicationBuilder builder) where TDbContext : FrameworkDbContext
        {
            return builder.UseMiddleware<SessionValidationMiddleware<TDbContext>>();
        }
    }
}

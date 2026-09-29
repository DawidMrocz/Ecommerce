using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MroczwareFramework.Data;
using System.Security.Claims;

namespace MroczwareFramework.Middleware
{
    /// <summary>
    /// Middleware do walidacji sesji użytkownika i SecurityStamp
    /// </summary>
    public class SessionValidationMiddleware<TDbContext> where TDbContext : FrameworkDbContext
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public SessionValidationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var sessionId = context.User.FindFirstValue("SessionId");
                var securityStamp = context.User.FindFirstValue("SecurityStamp");

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(securityStamp))
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await _next(context);
                    return;
                }

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
                var session = await dbContext.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId);

                if (user == null || session == null || user.SecurityStamp != securityStamp || session.ExpiresAt < DateTime.UtcNow)
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await _next(context);
                    return;
                }

                session.ExpiresAt = DateTime.UtcNow.AddMinutes(15);
                await dbContext.SaveChangesAsync();
            }

            await _next(context);
        }
    }
}

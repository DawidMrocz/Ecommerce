using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MroczwareFramework.Middlewares
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Add("Content-Security-Policy", new StringValues("default-src 'none'; style-src 'self'; img-src 'self'; font-src 'self'; script-src 'self'"));
            context.Response.Headers.Add("X-Content-Type-Options", new StringValues("nosniff"));
            context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", new StringValues("master-only"));
            context.Response.Headers.Add("X-Frame-Options", new StringValues("SAMEORIGIN"));
            context.Response.Headers.Add("X-XSS-Protection", new StringValues("1; mode=block"));
            if (!context.Response.HasStarted)
            {
                context.Response.Headers["Server"] = string.Empty;
            }

            await _next(context);
        }
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MroczwareFramework.Data;
using MroczwareFramework.Models.User;
using MroczwareFramework.Services.Authentication;
using MroczwareFramework.Services.User;

namespace MroczwareFramework.Extensions
{
    public static class AddMroczwareCookieAuthExtension
    {
        public static AuthenticationBuilder AddMroczwareCookieAuth<TUserModel, TDbContext>(this IServiceCollection services, bool isDevelopment) where TUserModel : UserModel, new() where TDbContext : FrameworkDbContext
        {
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddScoped<IMroczwareUserService, MroczwareUserService<TDbContext>>();
            services.AddScoped<IAuthenticationCookie, AuthenticationCookie<TUserModel, TDbContext>>();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(15);
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
                options.Cookie.Name = "SessionCookie";
            });

            var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "AuthCookie";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                options.SlidingExpiration = true;

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.Headers["Location"] = string.Empty;
                        return Task.CompletedTask;
                    }
                };
            });
            return authBuilder;
        }
    }
}

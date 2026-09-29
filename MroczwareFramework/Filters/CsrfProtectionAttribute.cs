using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace MroczwareFramework.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CsrfProtectionAttribute : Attribute, IAsyncActionFilter
    {
        private const string CsrfHeaderName = "X-CSRF-TOKEN";
        private const string CsrfClaimType = "CSRF-TOKEN";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;

            // Pomiń akcje AllowAnonymous
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            {
                await next();
                return;
            }

            // Pomiń GET (CSRF dotyczy tylko mutujących requestów)
            if (http.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            // Pobierz token z headera
            var headerToken = http.Request.Headers[CsrfHeaderName].FirstOrDefault();
            if (string.IsNullOrEmpty(headerToken))
            {
                context.Result = new BadRequestObjectResult(new { error = "Brak tokenu CSRF w nagłówku" });
                return;
            }

            // Pobierz token z claims
            var userToken = http.User?.Claims.FirstOrDefault(c => c.Type == CsrfClaimType)?.Value;
            if (string.IsNullOrEmpty(userToken))
            {
                context.Result = new BadRequestObjectResult(new { error = "Brak tokenu CSRF w claims użytkownika" });
                return;
            }

            // Porównaj tokeny
            if (!string.Equals(headerToken, userToken, StringComparison.Ordinal))
            {
                context.Result = new BadRequestObjectResult(new { error = "Niepoprawny token CSRF" });
                return;
            }

            // Wszystko ok, przejdź do akcji
            await next();
        }
    }
}

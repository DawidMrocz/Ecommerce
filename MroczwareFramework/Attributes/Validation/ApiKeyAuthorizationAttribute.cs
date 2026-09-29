using log4net.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MroczwareFramework.Services.SettingService;
using System.Net;


namespace MroczwareFramework.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ApiKeyAuthorizationAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                ISettingService? _settingService = context.HttpContext.RequestServices.GetService<ISettingService>()
                    ?? throw new Exception("Setting service not found");

                string? apiKey = context.HttpContext.Request.Headers["x-api-key"].FirstOrDefault(x => !string.IsNullOrEmpty(x));
                if (apiKey is null)
                {
                    context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
                    return;
                }
                var apiKeyFromDb = await _settingService.Get("Veloce.ApiKey") ?? throw new Exception("Api key not found");

                if (!apiKey.Equals(apiKeyFromDb))
                {
                    context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
                    return;
                }

                await next();
            }
            catch (Exception ex)
            {
                ILogger? _logger = context.HttpContext.RequestServices.GetService<ILogger>();
               // _logger!.Log(GetType(), Level.Error, "Błąd podczas weryfikacji konta", ex);
                context.Result = new ContentResult
                {
                    Content = $"Client error: {ex.Message}",
                    ContentType = "text/plain",
                    StatusCode = (int?)HttpStatusCode.InternalServerError
                };
                return;
            }
        }
    }
}

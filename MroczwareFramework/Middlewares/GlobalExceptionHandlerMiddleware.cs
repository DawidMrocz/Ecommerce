using Microsoft.AspNetCore.Http;
using MroczwareFramework.Controllers;
using MroczwareFramework.Exceptions;
using System.Text;

namespace MroczwareFramework.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _provider;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, IServiceProvider provider)
        {
            _next = next;
            _provider = provider;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string originalRequestBodyString = await GetRequestBody(httpContext.Request);

            try
            {
                httpContext.Request.EnableBuffering();
                await _next(httpContext);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(httpContext, exception, originalRequestBodyString);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception, string originalRequestBodyString)
        {
            httpContext.Response.ContentType = "application/json";

            //var logger =
            //    (Veloce.Framework.Infrastructure.Logger.ILogger?)
            //    httpContext.RequestServices.GetService(typeof(Veloce.Framework.Infrastructure.Logger.ILogger));

            if (exception is NotFoundException)
            {
                await WriteResponse(httpContext, 404,
                    "Nie znaleziono rekordu o podanym parametrze");
                return;
            }

            if (exception is UnauthorizedException)
            {
                await WriteResponse(httpContext, 401,
                    string.IsNullOrWhiteSpace(exception.Message)
                        ? "Użytkownik nie jest zalogowany"
                        : exception.Message);
                return;
            }

            //if (exception is ForbiddenException)
            //{
            //    await WriteResponse(httpContext, 403,
            //        string.IsNullOrWhiteSpace(exception.Message)
            //            ? "Brak dostępu"
            //            : exception.Message);
            //    return;
            //}

            if (exception is BadRequestException)
            {
                await WriteResponse(httpContext, 400, exception.Message);
                return;
            }

            //logger?.Error(GetType(), "Exception occurred", exception);

            //using var scope = _provider.CreateScope();
            //var settingService = scope.ServiceProvider.GetRequiredService<ISettingService>();
            //var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
            //var templateRepository = scope.ServiceProvider.GetRequiredService<ITemplateRepository>();
            //var mongoRepository = scope.ServiceProvider.GetRequiredService<IMongoRepository>();

            //try
            //{
            //    var url = httpContext.Request.GetEncodedUrl();
            //    var uri = new Uri(url);
            //    var query = HttpUtility.ParseQueryString(uri.Query);
            //    Dictionary<string, IEnumerable<string>> queryDict = query.AllKeys
            //        .OfType<string>()
            //        .ToDictionary(key => key, key => query.GetValues(key)?.OfType<string>() ?? Array.Empty<string>());

            //    var endpoint = httpContext.GetEndpoint();
            //    string? controller = null;
            //    string? action = null;

            //    if (endpoint != null)
            //    {
            //        controller = endpoint.Metadata
            //            .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
            //            .FirstOrDefault()?.ControllerName;

            //        action = endpoint.Metadata
            //            .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
            //            .FirstOrDefault()?.ActionName;
            //    }

            //    var model = new LogModel()
            //    {
            //        Host = uri.Authority,
            //        Path = httpContext.Request.Path.Value?.TrimEnd('/'),
            //        Url = url,
            //        Message = exception.Message,
            //        InnerMessage = exception.InnerException?.Message,
            //        Method = httpContext.Request.Method,
            //        Controller = controller,
            //        Action = action,
            //        Date = DateTime.UtcNow,
            //        StackTrace = exception.StackTrace,
            //        Query = queryDict,
            //        Request = BsonNull.Value
            //    };

            //    try
            //    {
            //        if (BsonDocument.TryParse(originalRequestBodyString, out var parsedRequest))
            //            model.Request = parsedRequest;
            //        else if (!string.IsNullOrWhiteSpace(originalRequestBodyString))
            //            model.Request = new BsonDocument { { "raw", originalRequestBodyString } };
            //    }
            //    catch { }

            //    await mongoRepository.Create(model);

            //    var mailFrom = await settingService.GetAsync("VeloceMailFrom")
            //        ?? throw new Exception("Nie ustawiono VeloceMailFrom settinga");

            //    var telemetryUrl = await settingService.GetAsync("SelfBillingTelemetryUrl")
            //        ?? throw new Exception("Nie ustawiono MailUrlForBackOffice settinga");

            //    if (string.IsNullOrWhiteSpace(telemetryUrl))
            //        throw new Exception("Brak url do telemetry api.");

            //    var template = await templateRepository.GetAsync(new TemplateGetModel()
            //    {
            //        CultureId = 1,
            //        StrongName = "Exception_Warning_Template"
            //    }) ?? throw new Exception($"Nie znaleziono template maila Exception_Warning_Template");

            //    string body = template.Body.Replace("{{Path}}", model.Path);
            //    body = body.Replace("{{Host}}", model.Host);
            //    body = body.Replace("{{Method}}", model.Method);
            //    body = body.Replace("{{Data}}", model.Date.ToString());
            //    body = body.Replace("{{StackTrace}}", model.StackTrace);
            //    body = body.Replace("{{Action}}", model.Action);
            //    body = body.Replace("{{Controller}}", model.Controller);
            //    body = body.Replace("{{Message}}", model.Message);
            //    body = body.Replace("{{Year}}", DateTime.UtcNow.Year.ToString());
            //    body = body.Replace("{{url}}", $"{telemetryUrl}/logs/{model.Id}");

            //    string subject = template.Subject.Replace("{{Method}}", model.Method);
            //    subject = subject.Replace("{{Controller}}", model.Controller);
            //    subject = subject.Replace("{{Action}}", model.Action);

            //    await SendEmailAsync(mailFrom, subject, body, mailService);
            //}
            //catch (Exception) { Console.WriteLine(exception.Message); }


            await WriteResponse(httpContext, 500,
                "Przepraszamy wystąpił błąd w naszym serwisie");
        }

        private async Task WriteResponse(HttpContext httpContext, int status, string message)
        {
            var errorModel = new VeloceApiResponse<string>
            {
                Status = status,
                Data = message,
                Errors = Enumerable.Empty<VeloceApiError>()
            };

            httpContext.Response.StatusCode = status;

            await httpContext.Response.WriteAsJsonAsync(errorModel);
        }


        public static async Task<string> GetRequestBody(HttpRequest request)
        {
            if (!request.Body.CanSeek)
                return string.Empty;
            request.Body.Position = 0;
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var body = Encoding.UTF8.GetString(buffer);
            request.Body.Position = 0;
            return body;
        }
    }
}

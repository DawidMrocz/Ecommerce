using log4net.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.Net;

namespace MroczwareFramework.Controllers
{
    public abstract class BaseController : Controller
    {
        private IHttpContextAccessor? _httpContextAccessor;
        private Services.LoggerService.ILogger? _logger;

        protected IHttpContextAccessor HttpContextAccessor => GetServiceLazy(ref _httpContextAccessor) ?? throw new ArgumentNullException(nameof(HttpContextAccessor));
        protected Services.LoggerService.ILogger Logger => GetServiceLazy(ref _logger) ?? throw new ArgumentNullException(nameof(Logger));

        private static class Messages
        {
            public const string TranslateError = "BaseController.T Nie udało się poprawnie przejść procesu tworzenia / pobierania translacji. Name: {0}";
        }

        public BaseController()
        {
        }

        protected T? GetService<T>()
        {
            return (T?)HttpContext?.RequestServices.GetService(typeof(T));
        }

        protected T? GetServiceLazy<T>(ref T? service)
        {
            return service ??= GetService<T>();
        }

        protected ModelStateDictionary T(ModelStateDictionary modelState)
        {
            var result = new ModelStateDictionary(modelState);
            result.Clear();
            foreach (var key in modelState.Keys)
            {
                ModelErrorCollection? errors = modelState[key]?.Errors;
                if (errors is null)
                    continue;
                foreach (var error in errors)
                {
                    result.AddModelError(key, error.ErrorMessage);
                }
            }

            return result;
        }

        protected virtual BadRequestObjectResult BadRequest(VeloceBusinessException ex)
        {
            var response = new VeloceApiResponse<object>
            {
                Errors = ex.Errors.Select(e => new VeloceApiError
                {
                    MessageWithParams = e.Message,

                    Fields = e.Fields?.Select(f => new VeloceApiFieldError
                    {
                        MessagesWithParams = f.Messages,
                        Name = f.Name
                    }) ?? Enumerable.Empty<VeloceApiFieldError>()
                }),
                Status = -1
            };
            return BadRequest(response);
        }

        protected virtual BadRequestObjectResult BadRequest<R>(VeloceApiResponse<R> response)
        {
            return base.BadRequest(response);
        }

        protected virtual BadRequestObjectResult BadRequest(string error)
        {
            var message = error;
            return base.BadRequest(new VeloceApiResponse<object>(message));
        }

        protected new virtual BadRequestObjectResult BadRequest(ModelStateDictionary modelState)
        {
            var response = new VeloceApiResponse<object>
            {
                Errors = new List<VeloceApiError>
                {
                    new()
                    {
                        Fields = ModelState.Keys.Select(k => new VeloceApiFieldError
                        {
                            Name = k,
                            MessagesWithParams = ModelState[k]!.Errors.Select(e =>new ParametrizedString(e.ErrorMessage ))
                        })
                    }
                }
            };

            return base.BadRequest(response);
        }

        protected virtual OkObjectResult Ok(string message)
        {
            message = message;
            return base.Ok(new VeloceApiResponse<string>
            {
                Errors = Enumerable.Empty<VeloceApiError>(),
                Data = message,
                Status = 0
            });
        }

        protected virtual OkObjectResult Ok<T>(T obj)
        {
            return base.Ok(new VeloceApiResponse<T>(obj));
        }

        protected virtual ObjectResult ServerError(string error, Exception ex)
        {
            var message = error;
            LogError(message, ex);
            return StatusCode((int)HttpStatusCode.InternalServerError, new VeloceApiResponse<int>(message));
        }

        protected virtual ObjectResult ServerError(Exception ex)
        {
            LogError(string.Empty, ex);
            return StatusCode((int)HttpStatusCode.InternalServerError, new VeloceApiResponse<int>());
        }

        private void LogError(string message, Exception ex)
        {
            try
            {
                Logger.Log(GetType(),Level.Critical, message, ex);
            }
            catch
            {
                Logger.Log(GetType(), Level.Critical, message, ex);
            }
        }
    }

    public class VeloceApiResponse<T>
    {
        public T? Data { get; set; }
        public int Status { get; set; }
        public IEnumerable<VeloceApiError> Errors { get; set; } = new List<VeloceApiError>();

        public VeloceApiResponse()
        {

        }

        public VeloceApiResponse(T data)
        {
            Data = data;
            Status = 0;
            Errors = Enumerable.Empty<VeloceApiError>();
        }

        public VeloceApiResponse(string error)
        {
            Status = -1;
            Errors = new List<VeloceApiError>
            {
                new() {
                    MessageWithParams = new ParametrizedString(error)
                }
            };
        }

        public VeloceApiResponse(string error, int status)
        {

            Status = status;
            Errors = new List<VeloceApiError>
            {
                new () {
                    MessageWithParams = new ParametrizedString(error)
                }
            };
        }

        public VeloceApiResponse(VeloceBusinessException ex)
        {
            Status = -1;
            Errors = ex.Errors?.Select(e => new VeloceApiError
            {
                Fields = e.Fields?.Select(f => new VeloceApiFieldError
                {
                    MessagesWithParams = f.Messages,
                    Name = f.Name
                }) ?? new List<VeloceApiFieldError>(),
                MessageWithParams = new ParametrizedString(e.Message.Value)
            }) ?? new List<VeloceApiError>();
        }

        public VeloceBusinessException ToBusinessException()
        {
            return new VeloceBusinessException
            {
                Errors = Errors?.Select(e => new VeloceBusinessError
                {
                    Fields = e.Fields?.Select(f => new VeloceBusinessFieldError
                    {
                        Messages = f.MessagesWithParams,
                        Name = f.Name
                    }) ?? new List<VeloceBusinessFieldError>(),
                    Message = e.MessageWithParams ?? new ParametrizedString(string.Empty)
                }) ?? new List<VeloceBusinessError>()
            };
        }
    }

    public class VeloceApiError
    {
        public ParametrizedString? MessageWithParams { get; set; }
        public IEnumerable<VeloceApiFieldError> Fields { get; set; } = new List<VeloceApiFieldError>();
    }


    public class VeloceApiFieldError
    {

        public required string Name { get; set; }
        public IEnumerable<ParametrizedString> MessagesWithParams { get; set; } = new List<ParametrizedString>();
    }

    public class VeloceBusinessException : ApplicationException
    {
        public IEnumerable<VeloceBusinessError> Errors { get; set; }

        public VeloceBusinessException()
        {
            Errors = new List<VeloceBusinessError>();
        }

        public VeloceBusinessException(string msg, Exception? ex = null) : base(msg, ex)
        {
            Errors = new List<VeloceBusinessError>
            {
                new VeloceBusinessError {
                    Message = new ParametrizedString(msg)
                }
            };
        }

        public VeloceBusinessException(string msg, params object[] parameters) : base(msg)
        {
            Errors = new List<VeloceBusinessError>
            {
                new VeloceBusinessError {
                    Message = new ParametrizedString(msg, parameters)
                }
            };
        }
    }

    public class ParametrizedString
    {
        public string Value { get; set; }
        public IEnumerable<object?> Params { get; set; } = Enumerable.Empty<object?>();

        //Dodaję, bo inaczej jak w JSON przyjdzie taki obiekt to System.Text.Json (którego teraz używamy) nie umie zdeserializować,
        //prawdopodobnie, bo nazwy parametrów konstruktora nie pokrywają się z nazwami pól (nie mogą, params jest słowem kluczowym)
        public ParametrizedString()
        {

        }

        public ParametrizedString(string value, params object?[] parameters)
        {
            Value = value;
            Params = parameters;
        }
    }

    public class VeloceBusinessError
    {
        public required ParametrizedString Message { get; init; }
        public IEnumerable<VeloceBusinessFieldError> Fields { get; init; } = Enumerable.Empty<VeloceBusinessFieldError>();
    }


    public class VeloceBusinessFieldError
    {
        public required string Name { get; init; }
        public IEnumerable<ParametrizedString> Messages { get; init; } = Enumerable.Empty<ParametrizedString>();
    }
}

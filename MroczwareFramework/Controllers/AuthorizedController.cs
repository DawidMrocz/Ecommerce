using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MroczwareFramework.DataModels.Authentication;
using MroczwareFramework.Filters;
using MroczwareFramework.Services.Authentication;

namespace MroczwareFramework.Controllers
{
    [CsrfProtection]
    [Authorize]
    [ApiController]
    public abstract class CookieAuthorizedController() : BaseController
    {
        protected string Culture => (HttpContext.Request.Cookies.TryGetValue("culture", out string? value) ? value : "pl-PL") ?? "pl-PL";
        protected AuthenticationUser? AuthorizedUser => AuthenticationService.GetAuthenticatedUser().Result;
        protected IAuthenticationCookie AuthenticationService => HttpContext?.RequestServices.GetService(typeof(IAuthenticationCookie)) as IAuthenticationCookie
                    ?? throw new Exception("Authorization provider not found");
    }
}

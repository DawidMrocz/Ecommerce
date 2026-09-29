using Gateway.Api.ApiModels.Access;
using Gateway.Api.ApiModels.Auth;
using Gateway.Api.ApiModels.Exceptions;
using Gateway.Api.Extensions;
using Gateway.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using static Gateway.Api.Controllers.Identity.IdentityController;

namespace Gateway.Api.Controllers.Identity
{
    [ApiController]
    [Route("[controller]")]
    public class AccessController : ControllerBase
    {
        private readonly IAuthKsefSessionService _authKsefSessionService;

        public AccessController(IAuthKsefSessionService authKsefSessionService)
        {
            _authKsefSessionService = authKsefSessionService;
        }

        /// <summary>
        /// Akcja do logowania
        /// </summary>
        [HttpPost("init-auth")]
        [AllowAnonymous]
        public async Task<IActionResult> InitAuth()
        {
           var response = await _authKsefSessionService.AuthenticateAsync();

            return Ok(response);
        }
    }
}



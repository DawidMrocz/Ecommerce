using Identity.Api.ApiModels;
using Identity.Api.DTO;
using Identity.Api.Models;
using Identity.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly IAuthenticationService _auth;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(IAuthenticationService auth, ILogger<IdentityController> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        /// <summary>
        /// Zwraca profil zalogowanego użytkownika
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<ProfileResponse>> Me()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized("Invalid JWT token");

                var profile = await _auth.Profile(int.Parse(userId));

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile for /me");
                return StatusCode(500, "Unexpected server error");
            }
        }

        /// <summary>
        /// Odświeżanie tokenów JWT
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var result = await _auth.RefreshToken(request.RefreshToken, ip);

                if (result == null)
                    return Unauthorized("Invalid or expired refresh token");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing JWT token");
                return StatusCode(500, "Unexpected server error");
            }
        }



        /// <summary>
        /// Logowanie użytkownika do systemu
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] ApiModels.LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var authResult = await _auth.Login(request, ip);
                if (authResult == null)
                    return Unauthorized("Invalid login or password");

                return Ok(authResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for email {Email}", request.Email);
                return StatusCode(500, "Unexpected server error");
            }
        }

        /// <summary>
        /// Dodanie zdjęcia użytkownika
        /// </summary>
        [HttpPost("photo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPhoto([FromForm] AddPhotoRequest request)
        {
            if (request.Photo == null || request.Photo.Length == 0)
                return BadRequest("Brak pliku");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null)
                return Unauthorized();

            try
            {
                await _auth.AddPhoto(request.Photo, int.Parse(userIdClaim.Value));
                return NoContent(); // 204 – standard dla uploadu
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd wysyłania pliku");
                return StatusCode(500, "Unexpected server error");
            }
        }

        /// <summary>
        /// Pobranie zdjęcia użytkownika
        /// </summary>
        [HttpGet("photo")]
        public async Task<IActionResult> GetPhoto()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null)
                return Unauthorized();

            try
            {
                var photo = await _auth.GetPhoto(int.Parse(userIdClaim.Value));

                if (photo.fileContent == null)
                    return NotFound();

                return File(photo.fileContent, photo.mimeType, photo.fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd pobierania pliku");
                return StatusCode(500, "Unexpected server error");
            }
        }

        /// <summary>
        /// Rejestracja użytkownika
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] ApiModels.RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _auth.Register(request);
                return Ok("User registered successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Business validation error during registration");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration");
                return StatusCode(500, "Unexpected server error");
            }
        }
    }
}

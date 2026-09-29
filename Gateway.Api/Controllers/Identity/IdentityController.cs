using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Gateway.Api.Controllers.Identity
{
    [ApiController]
    [Route("[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(IMemoryCache memoryCache, IHttpClientFactory clientFactory, ILogger<IdentityController> logger)
        {
            _memoryCache = memoryCache;
            _clientFactory = clientFactory;
            _logger = logger;
        }


        public record LoginRequest(string Email, string Password, string Platform);
        public record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresIn, DateTime RefreshTokenExpiresIn);
        public record SessionData(string AccessToken, string RefreshToken, string DeviceId, DateTime ExpiresAt);
        public record SessionResult(string SessionId, SessionData Data);

        /// <summary>
        /// Akcja do logowania
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var identityClient = _clientFactory.CreateClient("IdentityApi");

            // Używamy względnego URL względem BaseAddress
            var tokenResponse = await identityClient.PostAsJsonAsync("Identity/login", new
            {
                email = request.Email,
                password = request.Password
            });

            if (!tokenResponse.IsSuccessStatusCode)
                return Unauthorized();

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<AuthResponse>()
                ?? throw new Exception("Błąd deserializacji odpowiedzi tokena");

            var sessionId = Guid.NewGuid().ToString();
            var deviceId = Guid.NewGuid().ToString();
            var expiresAt = tokenData.RefreshTokenExpiresIn;
            var session = new SessionData(tokenData.AccessToken, tokenData.RefreshToken, deviceId, expiresAt);

            _memoryCache.Set(sessionId, session, tokenData.RefreshTokenExpiresIn);

            if (request.Platform.Equals("web", StringComparison.CurrentCultureIgnoreCase))
            {
                Response.Cookies.Append("sessionId", sessionId, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = expiresAt
                });
                return Ok(new { message = "Zalogowano webowo" });
            }
            else
            {
                return Ok(new { sessionId, deviceId, expiresAt });
            }
        }

        /// <summary>
        /// Akcja do wylogowania
        /// </summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var session = GetSession();
            if (session is null)
                return Unauthorized();

            if (session.SessionId != null)
            {
                _memoryCache.Remove(session.SessionId);
                if (Request.Cookies.ContainsKey("sessionId"))
                    Response.Cookies.Delete("sessionId");
            }

            return Ok(new { message = "Wylogowano" });
        }

        /// <summary>
        /// Akcja do odświeżania tokena
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var session = GetSession();
            if (session is null)
                return Unauthorized();

            var identityClient = _clientFactory.CreateClient("IdentityApi");

            var tokenResponse = await identityClient.PostAsJsonAsync("Identity/refresh", new
            {
                session.Data.RefreshToken
            });

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _memoryCache.Remove(session.SessionId);
                return Unauthorized();
            }

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<AuthResponse>();
            var expiresAt = tokenData.RefreshTokenExpiresIn;
            var updatedSession = new SessionData(tokenData.AccessToken, tokenData.RefreshToken, session.Data.DeviceId, expiresAt);

            _memoryCache.Set(session.SessionId, updatedSession, tokenData.RefreshTokenExpiresIn);

            if (Request.Cookies.ContainsKey("sessionId"))
                Response.Cookies.Append("sessionId", session.SessionId, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // w DEV nie wymuszamy HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = expiresAt
                });

            return Ok(new { message = "Token odświeżony", expiresAt });
        }

        /// <summary>
        /// Akcja do pobrania informacji o użytkowniku
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var session = GetSession();
            if (session is null)
                return Unauthorized();

            var identityClient = _clientFactory.CreateClient("IdentityApi");

            var request = new HttpRequestMessage(HttpMethod.Get, "Identity/me");

            // Dodajemy token
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", session.Data.AccessToken
            );

            var response = await identityClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting auth server");

            var profileJson = await response.Content.ReadAsStringAsync();

            return Content(profileJson, "application/json");
        }

        //[HttpPost("photo")]
        //public async Task<IActionResult> AddPhoto(IFormFile file)
        //{
        //    try
        //    {
        //        var session = GetSession();
        //        if (session is null)
        //            return Unauthorized();

        //        if (file == null || file.Length == 0)
        //            return BadRequest("Brak pliku");

        //        var identityClient = _clientFactory.CreateClient("IdentityApi");

        //        using var content = new MultipartFormDataContent();
        //        using var fileStream = file.OpenReadStream();
        //        var fileContent = new StreamContent(fileStream);
        //        fileContent.Headers.ContentType =
        //            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        //        content.Add(fileContent, "file", file.FileName);

        //        var request = new HttpRequestMessage(HttpMethod.Post, "Identity/photo")
        //        {
        //            Content = content
        //        };

        //        request.Headers.Authorization =
        //            new System.Net.Http.Headers.AuthenticationHeaderValue(
        //                "Bearer", session.Data.AccessToken
        //            );

        //        var response = await identityClient.SendAsync(request);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            _logger.LogWarning("Błąd przesyłania pliku do Identity API. Status: {StatusCode}", response.StatusCode);
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            _logger.LogWarning("Treść błędu: {ErrorContent}", errorContent);
        //            return StatusCode((int)response.StatusCode, "Błąd przesyłania pliku do Identity API");
        //        }

        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Wyjątek podczas przesyłania pliku do Identity API");
        //        return StatusCode(500, $"Unexpected server error: {ex.Message}");
        //    }
        //}

        //[HttpGet("photo")]
        //public async Task<IActionResult> GetPhoto()
        //{
        //    try
        //    {
        //        var session = GetSession();
        //        if (session is null)
        //            return Unauthorized();

        //        var identityClient = _clientFactory.CreateClient("IdentityApi");

        //        var request = new HttpRequestMessage(HttpMethod.Get, "Identity/photo");
        //        request.Headers.Authorization =
        //            new System.Net.Http.Headers.AuthenticationHeaderValue(
        //                "Bearer", session.Data.AccessToken
        //            );

        //        var response = await identityClient.SendAsync(
        //            request,
        //            HttpCompletionOption.ResponseHeadersRead
        //        );

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            _logger.LogWarning("Błąd pobierania pliku z Identity API. Status: {StatusCode}", response.StatusCode);
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            _logger.LogWarning("Treść błędu: {ErrorContent}", errorContent);
        //            return StatusCode((int)response.StatusCode, "Błąd pobierania pliku z Identity API");
        //        }

        //        var contentType =
        //            response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

        //        var fileName =
        //            response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "photo";

        //        var stream = await response.Content.ReadAsStreamAsync();

        //        return File(stream, contentType, fileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Wyjątek podczas pobierania pliku z Identity API");
        //        return StatusCode(500, $"Unexpected server error: {ex.Message}");
        //    }
        //}

        private SessionResult? GetSession()
        {
            string? sessionId = null;

            if (Request.Cookies.TryGetValue("sessionId", out var cookieValue))
                sessionId = cookieValue;
            else if (Request.Headers.TryGetValue("X-Session-Id", out var headerValue))
                sessionId = headerValue.ToString();

            if (string.IsNullOrWhiteSpace(sessionId))
                return null;

            if (!_memoryCache.TryGetValue(sessionId, out SessionData? session))
                return null;

            if (session is null)
                return null;

            if (session.ExpiresAt <= DateTime.UtcNow)
            {
                _memoryCache.Remove(sessionId);
                return null;
            }

            return new SessionResult(sessionId, session);
        }
    }
}


//# kubectl port-forward svc/ingress-nginx-controller 8080:80 -n ingress-nginx i w hosts 127.0.0.1  dashboard.com               
//# HorizontalPodAutoscaler (HPA)	Automatyczne skalowanie Deployment/StatefulSet w oparciu o CPU, pamięć lub custom metrics.
//# NetworkPolicy	Definiuje reguły sieciowe między Podami.
//# Role / ClusterRole i RBAC	Kontrola dostępu w klastrze (kto może co robić).
//# ServiceAccount	Specjalny “użytkownik” dla Podów, np. do autoryzacji w API Kubernetes.
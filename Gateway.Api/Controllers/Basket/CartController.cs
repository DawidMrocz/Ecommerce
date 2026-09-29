using Contracts.ApiModels.Basket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using static Gateway.Api.Controllers.Identity.IdentityController;

namespace Gateway.Api.Controllers.Identity
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _clientFactory;

        public CartController(IMemoryCache memoryCache, IHttpClientFactory clientFactory)
        {
            _memoryCache = memoryCache;
            _clientFactory = clientFactory;
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var session = GetSession();
            if (session is null)
                return Unauthorized();

            // Tworzymy klienta do CartApi
            var client = _clientFactory.CreateClient("CartApi");

            // Zakładamy, że CartApi ma endpoint GET /api/cart
            // i wymaga JWT w Authorization
            var request = new HttpRequestMessage(HttpMethod.Get, "Cart");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", session.Data.AccessToken
            );

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting Cart API");

            var cart = await response.Content.ReadFromJsonAsync<CartResponseDto>();

            return cart is null ? NotFound() : Ok(cart);
        }

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

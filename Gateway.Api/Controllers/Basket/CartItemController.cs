using Contracts.ApiModels.Basket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using static Gateway.Api.Controllers.Identity.IdentityController;

namespace Gateway.Api.Controllers.Identity
{
    [ApiController]
    [Route("[controller]")]
    public class CartItemController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _clientFactory;

        public CartItemController(IMemoryCache memoryCache, IHttpClientFactory clientFactory)
        {
            _memoryCache = memoryCache;
            _clientFactory = clientFactory;
        }

        [HttpPut("{cartItemId:int}")]
        public async Task<IActionResult> ChangeQuantity([FromRoute] int cartItemId, [FromBody] ChangeQuantityDto request)
        {
            var session = GetSession();
            if (session is null)
                return Unauthorized();

            var client = _clientFactory.CreateClient("CartApi");

            // Wywołanie CartApi PUT /api/cartitem/{id} z tokenem JWT
            var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"CartItem/{cartItemId}")
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", session.Data.AccessToken
            );

            var response = await client.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting Cart API");

            return Ok(new { message = "Quantity updated" });
        }

        // DELETE: api/cartitem/5
        [HttpDelete("{cartItemId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int cartItemId)
        {
            var session = GetSession();
            if (session is null)
                return Unauthorized();

            var client = _clientFactory.CreateClient("CartApi");

            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"CartItem/{cartItemId}");
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", session.Data.AccessToken
            );

            var response = await client.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting Cart API");

            return Ok(new { message = "Cart item deleted" });
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

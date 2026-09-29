using Contracts.ApiModels.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using static Gateway.Api.Controllers.Identity.IdentityController;

namespace Gateway.Api.Controllers.Product
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _clientFactory;
        public ProductController(IMemoryCache memoryCache, IHttpClientFactory clientFactory)
        {
            _memoryCache = memoryCache;
            _clientFactory = clientFactory;
        }

        // GET: api/Product/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetProductResponse>> Get(int id)
        {
            var session = GetSession();
            if (session is null)
                return Unauthorized();

            var client = _clientFactory.CreateClient("ProductApi");

            // Relatywny URL względem BaseAddress
            var response = await client.GetAsync($"Products/{id}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting Product API");

            var product = await response.Content.ReadFromJsonAsync<GetProductResponse>();
            return product is null ? NotFound() : Ok(product);
        }

        // POST: api/Product
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateProductRequest request)
        {
            var session = GetSession();
            if (session is null)
                return Unauthorized();

            var client = _clientFactory.CreateClient("ProductApi");
            var response = await client.PostAsJsonAsync("Products", request);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting Product API");

            var createdId = await response.Content.ReadFromJsonAsync<int>();
            return Ok(createdId);
        }

        // DELETE: api/Product/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var session = GetSession();
            if (session is null)
                return Unauthorized();

            var client = _clientFactory.CreateClient("ProductApi");
            var response = await client.DeleteAsync($"Products/{id}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting Product API");

            return Ok(new { message = "Product deleted" });
        }

        //// POST: api/Product/5/photo
        //[HttpPost("{id:int}/photo")]
        //public async Task<IActionResult> AddPhoto(int id, [FromForm] IFormFile request)
        //{
        //    var session = GetSession();
        //    if (session is null)
        //        return Unauthorized();

        //    var client = _clientFactory.CreateClient("ProductApi");

        //    // Multipart form-data
        //    using var form = new MultipartFormDataContent();
        //    if (request != null)
        //    {
        //        var streamContent = new StreamContent(request.OpenReadStream());
        //        streamContent.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
        //        form.Add(streamContent, "Photo", request.FileName);
        //    }

        //    var response = await client.PutAsync($"Products/{id}/photo", form);

        //    if (!response.IsSuccessStatusCode)
        //        return StatusCode((int)response.StatusCode, "Error contacting Product API");

        //    return Ok(new { message = "Photo uploaded" });
        //}

        //// DELETE: api/Product/5/photo
        //[HttpDelete("{id:int}/photo")]
        //public async Task<IActionResult> DeletePhoto(int id)
        //{
        //    var session = GetSession();
        //    if (session is null)
        //        return Unauthorized();

        //    var client = _clientFactory.CreateClient("ProductApi");
        //    var response = await client.DeleteAsync($"Products/{id}/photo");

        //    if (!response.IsSuccessStatusCode)
        //        return StatusCode((int)response.StatusCode, "Error contacting Product API");

        //    return Ok(new { message = "Photo deleted" });
        //}

        //[HttpGet("{id:int}/photo")]
        //public async Task<IActionResult> GetPhoto(int id)
        //{
        //    var session = GetSession();
        //    if (session is null)
        //        return Unauthorized();

        //    var client = _clientFactory.CreateClient("ProductApi");

        //    // Pobranie zdjęcia jako stream
        //    var response = await client.GetAsync($"Products/{id}/photo");

        //    if (!response.IsSuccessStatusCode)
        //        return StatusCode((int)response.StatusCode, "Error contacting Product API");

        //    // Odczyt strumienia
        //    var stream = await response.Content.ReadAsStreamAsync();

        //    // Pobranie Content-Type z odpowiedzi API
        //    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

        //    return File(stream, contentType);
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

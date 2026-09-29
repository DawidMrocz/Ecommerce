using Cart.Api.Services;
using Contracts.ApiModels.Basket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cart.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _service;

    public CartController(ICartService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<CartResponseDto>> GetByUser()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var basket = await _service.GetOrCreate(userId);
            return Ok(basket);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Wewnętrzny błąd serwera.");
        }
    }
}
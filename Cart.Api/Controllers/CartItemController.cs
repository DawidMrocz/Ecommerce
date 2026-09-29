using Cart.Api.Services;
using Contracts.ApiModels.Basket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cart.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CartItemController : ControllerBase
{
    private readonly ICartItemService _service;

    public CartItemController(ICartItemService service)
    {
        _service = service;
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ChangeQuantity([FromRoute] int id, [FromBody] ChangeQuantityDto dto)
    {
        try
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _service.ChangeQuantityAsync(id, dto.Quantity, userId);

            return Ok();
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest("Quantity nie może być ujemne.");
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Wewnętrzny błąd serwera.");
        }
    }

    // DELETE: api/basketitem/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.DeleteAsync(id, userId);

            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Wewnętrzny błąd serwera.");
        }
    }
}
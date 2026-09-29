using Contracts.ApiModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Api.ApiModel;
using Product.Api.Services;
using System.Security.Claims;

namespace Product.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/Models.Products/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetProductResponse>> Get(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            return product is null ? NotFound() : Ok(product);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Wewnętrzny błąd serwera.");
        }
    }

    // POST: api/Models.Products
    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var response = await _productService.CreateAsync(request);
            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Wewnętrzny błąd serwera.");
        }
    }

    // DELETE: api/Models.Products/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Wewnętrzny błąd serwera.");
        }
    }

    [HttpPost("basket/{id:int}")]
    [Authorize]
    public async Task<IActionResult> AddToBasket(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _productService.AddToBasketAsync(id, userId);
            return Ok("Produkt został dodany");
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Wewnętrzny błąd serwera.");
        }
    }

    [HttpPut("{id:int}/photo")]
    [Authorize]
    public async Task<IActionResult> AddPhoto([FromRoute] int id, [FromForm] AddProductPhotoRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _productService.AddPhotoAsync(request, id, userId);
            return Ok("Produkt został dodany");
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Wewnętrzny błąd serwera.");
        }
    }

    [HttpGet("{id:int}/photo")]
    public async Task<IActionResult> GetPhoto(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var photoResult = await _productService.GetPhotoAsync(id, userId);

            // Zwracamy plik z odpowiednim Content-Type
            return File(photoResult.content, photoResult.contentType, photoResult.fileName);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Wewnętrzny błąd serwera.");
        }
    }
}

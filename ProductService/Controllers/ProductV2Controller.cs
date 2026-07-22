using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/{version:apiVersion}/Product")]
[Authorize]
public class ProductV2Controller : ControllerBase
{
    private readonly IProductService _service;

    public ProductV2Controller(IProductService service)
    {
        _service = service;
    }

    // GET: api/2/Product
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductV2Dto>>> GetProducts()
    {
        var products = await _service.GetProductsV2Async();

        return Ok(products);
    }

    // GET: api/2/Product/5
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var products = await _service.GetProductsV2Async();

        var product = products.FirstOrDefault(x => x.Id == id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }
}
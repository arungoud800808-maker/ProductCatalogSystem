using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands.Products.CreateProduct;
using ProductService.Application.Commands.Products.DeleteProduct;
using ProductService.Application.Commands.Products.UpdateProduct;
using ProductService.Application.Queries.Products.GetProductById;
using ProductService.Application.Queries.Products.GetProducts;
using ProductService.Constants;
using ProductService.DTOs;
using ProductService.Services;
using ProductService.Wrappers;

namespace ProductService.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{version:apiVersion}/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IMediator _mediator;
    public ProductController(
    IMediator mediator,
    IProductService service)
    {
        _mediator = mediator;
        _service = service;
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        var imagePath = await _service.UploadImageAsync(image);

        return Ok(new
        {
            ImageUrl = imagePath
        });
    }
    [AllowAnonymous]
    [HttpGet("filter")]
    public async Task<IActionResult> Filter(decimal minPrice, decimal maxPrice)
    {
        var products = await _service.FilterProductsAsync(minPrice, maxPrice);

        return Ok(new ApiResponse<IEnumerable<ProductDto>>(
    true,
    "Products filtered successfully.",
    products));
    }

    [AllowAnonymous]
    [HttpGet("sort")]
    public async Task<IActionResult> SortProducts(string sortBy)
    {
        var products = await _service.SortProductsAsync(sortBy);

        return Ok(new ApiResponse<IEnumerable<ProductDto>>(
      true,
      "Products sorted successfully.",
      products));
    }

    [AllowAnonymous]
    //[Authorize]
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedProducts(
    int pageNumber = 1,
    int pageSize = 5)
    {
        var products = await _service.GetPagedProductsAsync(pageNumber, pageSize);
        return Ok(new ApiResponse<IEnumerable<ProductDto>>(
     true,
     "Products retrieved successfully.",
     products));
    }
    [AllowAnonymous]
    //[Authorize]
    [HttpGet("search")]
    public async Task<IActionResult> Search(string keyword)
    {
        var products = await _service.SearchProductsAsync(keyword);

        return Ok(new ApiResponse<IEnumerable<ProductDto>>(
    true,
    "Search completed successfully.",
    products));
    }


    // GET: api/Product
    // Accessible without login
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product =
            await _mediator.Send(new GetProductByIdQuery(id));

        if (product == null)
            return NotFound(new
            {
                Message = $"Product with Id {id} not found."
            });

        return Ok(new ApiResponse<ProductDto>(
     true,
     "Product retrieved successfully.",
     product));
    }
    [AllowAnonymous]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _service.GetProductsAsync();

        return Ok(products);
    }
    // GET: api/1/Product
    [AllowAnonymous]
    [HttpGet]
    [MapToApiVersion("1.0")]
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        string? search,
        string? sort,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var products = await _mediator.Send(
            new GetProductsQuery(
                search,
                sort,
                pageNumber,
                pageSize));

        return Ok(new ApiResponse<IEnumerable<ProductDto>>(
     true,
     "Products retrieved successfully.",
     products));
    }
    [AllowAnonymous]
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        var products = await _service.GetProductsByCategoryAsync(categoryId);

        if (!products.Any())
        {
            return NotFound(new ApiResponse<object>(
     false,
     "No products found for this category.",
     null));
        }

        return Ok(new ApiResponse<IEnumerable<ProductDto>>(
     true,
     "Category products retrieved successfully.",
     products));
    }


    // POST: api/Product
    // Login Required
    [Authorize(Policy = Permissions.ProductCreate)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductDto>> Create(
     [FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _mediator.Send(
            new CreateProductCommand(dto));
        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            new ApiResponse<ProductDto>(
                true,
                "Product created successfully.",
                product));
    }
    // PUT: api/Product/1
    // Login Required
    [Authorize(Policy = Permissions.ProductUpdate)]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Update(
    int id,
    [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _mediator.Send(
            new UpdateProductCommand(id, dto));

        if (product == null)
        {
            return NotFound(new
            {
                Message = $"Product with Id {id} not found."
            });
        }

        return Ok(new ApiResponse<ProductDto>(
     true,
     "Product updated successfully.",
     product));
    }
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkInsert([FromBody] BulkProductDto dto)
    {
        if (dto == null || dto.Products == null || !dto.Products.Any())
            return BadRequest("Products list cannot be empty.");

        await _service.BulkInsertAsync(dto);

        return Ok(new ApiResponse<object>(
    true,
    $"{dto.Products.Count} Products inserted successfully.",
    null));
    }

    [HttpPut("bulk-update")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkUpdate([FromBody] UpdateBulkProductDto dto)
    {
        if (dto.Products == null || !dto.Products.Any())
            return BadRequest("Products list cannot be empty.");

        await _service.BulkUpdateAsync(dto);

        return Ok(new ApiResponse<object>(
      true,
      $"{dto.Products.Count} Products updated successfully.",
      null));
    }

    // DELETE: api/Product/1
    // Login Required
    [Authorize(Policy = Permissions.ProductDelete)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _mediator.Send(
            new DeleteProductCommand(id));

        if (!deleted)
        {
            return NotFound(new
            {
                Message = $"Product with Id {id} not found."
            });
        }

        return Ok(new ApiResponse<string>(
     true,
     "Product deleted successfully.",
     null));
    }
    [HttpDelete("bulk-delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteDto dto)
    {
        if (dto.ProductIds == null || !dto.ProductIds.Any())
            return BadRequest("Product Id list cannot be empty.");

        await _service.BulkDeleteAsync(dto);

        return Ok(new ApiResponse<object>(
    true,
    $"{dto.ProductIds.Count} Products deleted successfully.",
    null));
    }
    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportProducts()
    {
        var file = await _service.ExportProductsAsync();

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Products.xlsx");
    }
    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ImportProducts(IFormFile file)
    {
        await _service.ImportProductsAsync(file);

        return Ok(new ApiResponse<object>(
    true,
    "Products imported successfully.",
    null));
    }
    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Dashboard()
    {
        var dashboard = await _service.GetDashboardAsync();

        return Ok(dashboard);
    }
}
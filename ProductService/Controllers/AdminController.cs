using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Constants;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Repositories.UnitOfWork;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.AuditView)]
public class AdminController : ControllerBase
{
    private readonly ProductDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public AdminController(
        ProductDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        return Ok("Only Admin can see this.");
    }

    [HttpPost("unlock-user/{email}")]
    public IActionResult UnlockUser(string email)
    {
        return Ok($"Unlocked {email}");
    }

    [HttpGet("auditlogs")]
    public async Task<IActionResult> AuditLogs()
    {
        var logs = await _unitOfWork.AuditLogs.GetAllAsync();

        return Ok(logs);
    }

    [HttpGet("deleted-products")]
    public async Task<IActionResult> GetDeletedProducts()
    {
        var products = await _context.Products
            .IgnoreQueryFilters()
            .Where(p => p.IsDeleted)
            .ToListAsync();

        var result = _mapper.Map<List<DeletedProductDto>>(products);

        return Ok(result);
    }

    [HttpPut("restore-product/{id}")]
    public async Task<IActionResult> RestoreProduct(int id)
    {
        var product = await _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound(new
            {
                Message = $"Product with Id {id} not found."
            });
        }

        product.IsDeleted = false;
        product.DeletedAt = null;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Product restored successfully."
        });
    }

    [HttpDelete("permanent-product/{id}")]
    public async Task<IActionResult> PermanentDelete(int id)
    {
        var product = await _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound(new
            {
                Message = $"Product with Id {id} not found."
            });
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Product permanently deleted successfully."
        });
    }
}
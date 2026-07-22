using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Constants;
using ProductService.DTOs;
using ProductService.Services;
using System.Security.Claims;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _service;

    public WishlistController(IWishlistService service)
    {
        _service = service;
    }

    // POST: api/Wishlist
    [HttpPost]
    [Authorize(Roles = $"{Roles.Customer},{Roles.Manager},{Roles.Admin}")]
    public async Task<IActionResult> Add(CreateWishlistDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        await _service.AddAsync(userId, dto);

        return Ok("Product added to wishlist.");
    }

    // GET: api/Wishlist
    [HttpGet]
    [Authorize(Roles = $"{Roles.Customer},{Roles.Manager},{Roles.Admin}")]
    public async Task<IActionResult> Get()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        var items = await _service.GetByUserAsync(userId);

        return Ok(items);
    }

    // DELETE: api/Wishlist/1
    [HttpDelete("{id}")]
    [Authorize(Roles = $"{Roles.Customer},{Roles.Manager},{Roles.Admin}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.RemoveAsync(id);

        return NoContent();
    }
}
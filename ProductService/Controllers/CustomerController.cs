using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Constants;
using ProductService.Data;
using System.Security.Claims;
using ProductService.DTOs;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Customer)]
    public class CustomerController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public CustomerController(ProductDbContext context)
        {
            _context = context;
        }

        // GET: api/customer/profile
        [Authorize(Roles = Roles.Customer)]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.CreatedDate
            });
        }

        // PUT: api/customer/profile
        [Authorize(Roles = Roles.Customer)]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return NotFound();

            user.FullName = dto.FullName;

            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully.");
        }

        // GET: api/customer/orders
        [Authorize(Roles = Roles.Customer)]
        [HttpGet("orders")]
        public IActionResult GetOrders()
        {
            return Ok("Customer Orders");
        }

        // GET: api/customer/wishlist
        [Authorize(Roles = Roles.Customer)]
        [HttpGet("wishlist")]
        public IActionResult Wishlist()
        {
            return Ok("Customer Wishlist");
        }

        // GET: api/customer/reviews
        [Authorize(Roles = Roles.Customer)]
        [HttpGet("reviews")]
        public IActionResult MyReviews()
        {
            return Ok("Customer Reviews");
        }
    }
}
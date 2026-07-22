using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Constants;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
public class ManagerController : ControllerBase
{
    [HttpGet("reports")]
    public IActionResult Reports()
    {
        return Ok();
    }

    [HttpPost("inventory")]
    public IActionResult UpdateInventory()
    {
        return Ok();
    }
}
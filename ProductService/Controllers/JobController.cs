using Hangfire;
using Microsoft.AspNetCore.Mvc;
using ProductService.Jobs;

namespace ProductService.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobController : ControllerBase
{
    [HttpPost("email")]
    public IActionResult SendEmail(
        [FromServices] IBackgroundJobClient jobs,
        string email)
    {
        jobs.Enqueue<EmailJobs>(
            x => x.SendWelcomeEmail(email));

        return Ok("Background job queued.");
    }
}
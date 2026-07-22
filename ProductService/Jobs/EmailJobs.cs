using Microsoft.Extensions.Logging;

namespace ProductService.Jobs;

public class EmailJobs
{
    private readonly ILogger<EmailJobs> _logger;

    public EmailJobs(ILogger<EmailJobs> logger)
    {
        _logger = logger;
    }

    public Task SendWelcomeEmail(string email)
    {
        _logger.LogInformation("Sending welcome email to {Email}", email);

        Console.WriteLine($"Welcome email sent to {email}");

        return Task.CompletedTask;
    }
    public Task SendDailyReport()
    {
        _logger.LogInformation("==================================");
        _logger.LogInformation("DAILY REPORT JOB STARTED");
        _logger.LogInformation("Daily report generated successfully.");
        _logger.LogInformation("DAILY REPORT JOB COMPLETED");
        _logger.LogInformation("==================================");
        Console.WriteLine("Daily report executed");

        return Task.CompletedTask;
   
    }
}
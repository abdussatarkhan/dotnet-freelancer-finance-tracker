using FreelancerTrack.Application.Features.RecurringProfiles.Commands.ProcessRecurringProfiles;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FreelancerTrack.Infrastructure.BackgroundJobs;

public class RecurringInvoiceBillingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringInvoiceBillingWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public RecurringInvoiceBillingWorker(
        IServiceProvider serviceProvider,
        ILogger<RecurringInvoiceBillingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recurring Invoice Billing Worker started.");

        // Initial delay to allow the application and database to fully initialize
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Scanning PostgreSQL for due recurring billing profiles at {Time}...", DateTimeOffset.UtcNow);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var result = await mediator.Send(new ProcessRecurringProfilesCommand(), stoppingToken);

                    if (result.IsSuccess)
                    {
                        var data = result.Value;
                        if (data.ProcessedProfilesCount > 0)
                        {
                            _logger.LogInformation(
                                "Successfully generated {Count} recurring invoice(s) for due profiles.",
                                data.ProcessedProfilesCount);
                        }
                        else
                        {
                            _logger.LogDebug("No recurring profiles were due for invoice generation.");
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to process recurring profiles: {Error}", result.Error);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "An unhandled error occurred while processing recurring invoice billing schedules.");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Recurring Invoice Billing Worker stopped.");
    }
}

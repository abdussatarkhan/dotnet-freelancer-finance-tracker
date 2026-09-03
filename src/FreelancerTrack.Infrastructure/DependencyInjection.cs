using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Infrastructure.BackgroundJobs;
using FreelancerTrack.Infrastructure.Persistence;
using FreelancerTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FreelancerTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<ICurrencyExchangeService, CurrencyExchangeService>();

        string storagePath = configuration["FileStorage:BasePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "ReceiptStorage");
        services.AddSingleton<IFileStorageService>(_ => new LocalFileStorageService(storagePath));

        // Background worker for automated recurring invoices
        services.AddHostedService<RecurringInvoiceBillingWorker>();

        return services;
    }
}

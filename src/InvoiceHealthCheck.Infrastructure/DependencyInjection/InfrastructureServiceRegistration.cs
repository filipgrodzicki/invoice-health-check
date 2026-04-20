using InvoiceHealthCheck.Application.Abstractions.ExchangeRates;
using InvoiceHealthCheck.Infrastructure.ExchangeRates;
using InvoiceHealthCheck.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace InvoiceHealthCheck.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("AppDatabase")));

        var frankfurterBaseUrl = configuration["Frankfurter:BaseUrl"]
            ?? throw new InvalidOperationException("Frankfurter:BaseUrl configuration is missing.");

        services
            .AddRefitClient<IFrankfurterApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(frankfurterBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        services.AddScoped<IExchangeRateService, FrankfurterExchangeRateService>();

        return services;
    }
}
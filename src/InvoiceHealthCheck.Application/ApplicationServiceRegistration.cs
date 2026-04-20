using System.Reflection;
using InvoiceHealthCheck.Application.Anomalies;
using InvoiceHealthCheck.Application.Anomalies.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceHealthCheck.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<IAnomalyRule, OutlierAmountRule>();
        services.AddScoped<IAnomalyRule, DuplicateDetectionRule>();
        services.AddScoped<IAnomalyRule, SanityCheckRule>();

        return services;
    }
}
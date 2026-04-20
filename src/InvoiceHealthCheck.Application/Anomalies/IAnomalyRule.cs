using InvoiceHealthCheck.Domain.ValueObjects;

namespace InvoiceHealthCheck.Application.Anomalies;

public interface IAnomalyRule
{
    string Name { get; }

    IEnumerable<AnomalyFlag> Evaluate(AnomalyCheckContext context);
}
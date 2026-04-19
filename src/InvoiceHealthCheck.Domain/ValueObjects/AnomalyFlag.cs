using InvoiceHealthCheck.Domain.Enums;

namespace InvoiceHealthCheck.Domain.ValueObjects;

public sealed record AnomalyFlag(
    string RuleName,
    AnomalySeverity Severity,
    string Message);
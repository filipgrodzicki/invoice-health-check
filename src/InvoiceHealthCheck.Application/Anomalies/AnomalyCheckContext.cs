namespace InvoiceHealthCheck.Application.Anomalies;

public sealed record AnomalyCheckContext(
    InvoiceCandidate Candidate,
    ContractorHistory History);
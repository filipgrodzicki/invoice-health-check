namespace InvoiceHealthCheck.Domain.Entities;

public class Invoice
{
    public Guid Id { get; private set; }
    public Guid ContractorId { get; private set; }
    public string InvoiceNumber { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public decimal VatRate { get; private set; }
    public DateTime IssueDate { get; private set; }
    public decimal? AmountInPln { get; private set; }
    public decimal? ExchangeRateUsed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Contractor Contractor { get; private set; } = default!;

    private Invoice() { }

    public Invoice(
        Guid contractorId,
        string invoiceNumber,
        decimal amount,
        string currency,
        decimal vatRate,
        DateTime issueDate)
    {
        if (contractorId == Guid.Empty)
            throw new ArgumentException("Contractor ID is required.", nameof(contractorId));
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number is required.", nameof(invoiceNumber));
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be 3 letters (ISO 4217).", nameof(currency));
        if (vatRate < 0 || vatRate > 1)
            throw new ArgumentException("VAT rate must be between 0 and 1 (e.g., 0.23 for 23%).", nameof(vatRate));

        Id = Guid.NewGuid();
        ContractorId = contractorId;
        InvoiceNumber = invoiceNumber.Trim();
        Amount = amount;
        Currency = currency.ToUpperInvariant();
        VatRate = vatRate;
        IssueDate = issueDate;
        CreatedAt = DateTime.UtcNow;
    }

    public void ApplyExchangeRate(decimal rate)
    {
        if (rate <= 0)
            throw new ArgumentException("Exchange rate must be positive.", nameof(rate));

        ExchangeRateUsed = rate;
        AmountInPln = Math.Round(Amount * rate, 2, MidpointRounding.AwayFromZero);
    }
}
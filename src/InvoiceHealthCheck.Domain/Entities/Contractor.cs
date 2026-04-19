namespace InvoiceHealthCheck.Domain.Entities;

public class Contractor
{
    public Guid Id { get; private set; }
    public string Nip { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string CountryCode { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    public ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

    private Contractor() { }

    public Contractor(string nip, string name, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(nip))
            throw new ArgumentException("NIP is required.", nameof(nip));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            throw new ArgumentException("Country code must be 2 letters (ISO 3166-1 alpha-2).", nameof(countryCode));

        Id = Guid.NewGuid();
        Nip = nip.Trim();
        Name = name.Trim();
        CountryCode = countryCode.ToUpperInvariant();
        CreatedAt = DateTime.UtcNow;
    }
}
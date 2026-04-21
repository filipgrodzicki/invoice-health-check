# InvoiceHealthCheck

My solution for the Symfonia Young Tech Challenge recruitment task. Instead of wrapping some random public API in a CRUD, I wanted to build something closer to what Symfonia actually does - invoicing and bookkeeping software.

The app helps a bookkeeper who gets a batch of invoices from foreign contractors and wants to check for obvious mistakes or duplicates before entering them into the ERP. She sends the batch as JSON to an analyze endpoint and gets back a report with flags - which invoice looks suspicious and why.

Public API used from the public-apis repo: **Frankfurter** - exchange rates from the European Central Bank

## What it actually checks

Four rules run for each invoice in the batch.

The first one looks at the amount. If a contractor usually invoices around 2500 EUR and there's one for 28000 EUR in the batch, it's probably a typo - someone added a zero. The rule compares the new amount with the median of the contractor's historical invoices in the same currency. If it's 3x higher or 3x lower, it gets flagged.

The second one looks for duplicates. If there's already an invoice in the database from the same contractor, same amount, same currency, with a date within plus-minus seven days - flag it. If the invoice number matches too, it's an exact duplicate and gets Error severity instead of Warning.

The third one checks if the currency makes sense for this contractor. A German always invoicing in EUR suddenly sends one in USD? Might be a mistake, might be invoice phishing - someone pretending to be the contractor and hoping the bookkeeper changes the bank account number.

The fourth one is basic sanity checks - negative amounts, VAT rate above 30%, issue date two years in the future, date from eight months ago. Those usually mean data entry errors.

Each rule can return zero, one or multiple warnings. The analyze endpoint collects them all and returns a report per invoice plus a summary at the top.

## How to run it

You need .NET 10 SDK. SQLite is created as a local file on first run.

```
git clone https://github.com/filipgrodzicki/invoice-health-check.git
cd invoice-health-check
dotnet run --project src/InvoiceHealthCheck.Api
```

On first run it creates the database, applies the migration, seeds three sample contractors (German EUR, US USD, Czech CZK) with a few historical invoices each, and opens Swagger UI on the root URL. You can immediately send a batch analyze request and see the flags without adding anything manually.

## Endpoints

There are three. POST `/api/invoices` adds a new invoice, creates the contractor if it doesn't exist yet, and converts the amount to PLN using the rate from the issue date. POST `/api/invoices/batch/analyze` is the main one - runs all rules on a batch and returns the report. It does not modify the database. GET `/api/invoices/contractors/{nip}/stats` returns invoice count, median and average amounts, and currencies used for a given contractor.

## Architecture

I went with Clean Architecture. Four projects. Domain has entities (Invoice, Contractor) and value objects (AnomalyFlag) with no external dependencies. Application has CQRS handlers, the rule abstractions, and interfaces like IExchangeRateService. Infrastructure has the EF Core DbContext, the Frankfurter HTTP client using Refit, and DI registration. Api has controllers, Program.cs and the exception middleware. Rules live in Application.

For the MediatR handlers I used IEnumerable<IAnomalyRule> injection - the detector doesn't know how many rules exist, it just iterates. Adding a fifth rule means adding one class and one DI registration, the detector doesn't change. Open-closed in practice.

## Why these choices

I picked SQLite because the task says "ready to run after clone". SQLite creates itself as a file, no container, no server. Switching to a real database later is a one-line change in InfrastructureServiceRegistration.

I picked Frankfurter over NBP even though NBP would be the official source for Polish VAT. Frankfurter needs no API key and the documentation is simpler. For production-grade Polish VAT compliance I would switch to NBP.

## Tests

Run `dotnet test`. 22 unit tests covering the four rules, including edge cases like empty history, currency mismatches, case-insensitive matching, and multiple flags from a single rule.
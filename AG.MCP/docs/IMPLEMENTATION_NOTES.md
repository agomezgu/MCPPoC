# Eggs Company Invoicing API - Implementation Notes

## Project Structure (Clean Architecture)

```
AG.MCP.Api/                    # Presentation layer
├── Controllers/               # REST API controllers
├── Filters/                   # Validation, exception handling
└── Program.cs                 # App bootstrap, DI

AG.MCP.Domain/                 # Core layer (no dependencies)
├── Entities/                  # Client, Invoice, InvoiceItem, Payment
└── Enums/                     # InvoiceStatus

AG.MCP.Application/            # Use cases layer
├── Common/                    # PagedResult, QueryParams, ApiError
├── DTOs/                      # Request/response models
├── Interfaces/                # Repository & service contracts
├── Services/                  # Business logic
└── Validators/                # FluentValidation rules

AG.MCP.Infrastructure/         # Data access layer
├── Data/                      # DbContext, migrations
├── Repositories/              # EF Core implementations
└── Seeders/                   # Initial data
```

## Invoice Status Calculation Logic

**Location:** `AG.MCP.Application/Services/InvoiceStatusCalculator.cs`

```csharp
// Rules (evaluated in order):
// 1. Cancelled stays cancelled
// 2. pendingAmount <= 0 → Paid
// 3. dueDate < today → Overdue
// 4. paidAmount > 0 → Partial
// 5. Else → Pending
```

Status is recalculated when:
- Creating an invoice
- Updating an invoice (no payments)
- Registering a payment (updates PaidAmount, PendingAmount, Status)

## Client Summary Calculation Logic

**Location:** `AG.MCP.Application/Services/ClientService.GetSummaryAsync`

| Metric | Calculation |
|--------|-------------|
| TotalInvoices | Count of all client invoices |
| PendingInvoices | Count where pendingAmount > 0, dueDate >= today |
| OverdueInvoices | Count where pendingAmount > 0, dueDate < today |
| TotalBilled | Sum of TotalAmount |
| TotalPaid | Sum of PaidAmount |
| TotalPending | Sum of PendingAmount |
| TotalOverdue | Sum of PendingAmount for overdue only |

## Accounts Receivable Summary Logic

**Location:** `AG.MCP.Application/Services/ReportService.GetAccountsReceivableAsync`

| Metric | Calculation |
|--------|-------------|
| TotalReceivable | Sum of all pending amounts |
| TotalOverdue | Sum of pending where dueDate < today |
| PendingInvoices | List with dueDate >= today |
| OverdueInvoices | List with dueDate < today, DaysOverdue computed |

## Sample Requests/Responses

### Create Client
```http
POST /api/clients
Content-Type: application/json

{
  "name": "Farm Fresh Co",
  "taxId": "TAX-004",
  "email": "orders@farmfresh.com",
  "phone": "+1-555-0199",
  "address": "100 Agri Way"
}
```

### Create Invoice
```http
POST /api/invoices
Content-Type: application/json

{
  "clientId": "11111111-1111-1111-1111-111111111101",
  "issueDate": "2025-03-20",
  "dueDate": "2025-04-20",
  "notes": "Monthly order",
  "items": [
    {
      "description": "White eggs - Large (30 count tray)",
      "productCode": "EGG-WL-30",
      "quantity": 50,
      "unit": "tray",
      "unitPrice": 8.50
    }
  ]
}
```

### Register Payment
```http
POST /api/payments
Content-Type: application/json

{
  "invoiceId": "22222222-2222-2222-2222-222222222202",
  "amount": 250.00,
  "paymentDate": "2025-03-20",
  "reference": "CHQ-003",
  "notes": "Second installment"
}
```

## Best Practices for Maintainability

1. **Single Responsibility** – Each service/repository handles one aggregate
2. **Interface Segregation** – Small, focused interfaces per entity
3. **Dependency Inversion** – Controllers depend on abstractions (interfaces)
4. **Explicit DTOs** – No domain entities in API responses
5. **Validation at the Edge** – FluentValidation before business logic
6. **Unit of Work** – Single SaveChanges per logical operation
7. **Pagination** – All list endpoints support `page`, `pageSize`, `search`, `sortBy`, `sortDescending`
8. **Consistent Error Responses** – `ApiError` with code, message, optional validation details

## Extensibility Points

- **New invoice statuses** – Add to `InvoiceStatus` enum, update `InvoiceStatusCalculator`
- **Additional report types** – Add methods to `IReportService`/`ReportService`
- **Different database** – Swap SQLite for SQL Server by changing `UseSqlite` to `UseSqlServer`
- **Authentication** – Add `[Authorize]` to controllers, register auth middleware
- **Audit fields** – Extend entities with CreatedBy, UpdatedBy, soft delete

## Running the API

```bash
dotnet run --project AG.MCP.Api
```

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Health: http://localhost:5000/health

## Connection String

Default: `Data Source=eggs_invoicing.db` (SQLite, file in project output).

Override in `appsettings.json` or environment:
```json
"ConnectionStrings": {
  "Default": "Data Source=/path/to/eggs.db"
}
```

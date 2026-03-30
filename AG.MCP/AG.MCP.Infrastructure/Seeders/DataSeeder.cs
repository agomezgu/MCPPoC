using AG.MCP.Domain.Entities;
using AG.MCP.Domain.Enums;
using AG.MCP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AG.MCP.Infrastructure.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken ct = default)
    {
        if (await context.Clients.AnyAsync(ct))
            return;

        var clients = new List<Client>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                Name = "Fresh Eggs Distributors Inc",
                TaxId = "TAX-001",
                Email = "contact@fresheggs.com",
                Phone = "+1-555-0101",
                Address = "123 Poultry Lane, Farmville",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                Name = "Sunrise Bakery LLC",
                TaxId = "TAX-002",
                Email = "orders@sunrisebakery.com",
                Phone = "+1-555-0102",
                Address = "456 Main Street, Downtown",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111103"),
                Name = "Metro Grocers",
                TaxId = "TAX-003",
                Email = "procurement@metrogrocers.com",
                Phone = "+1-555-0103",
                Address = "789 Market Ave",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        context.Clients.AddRange(clients);

        var now = DateTime.UtcNow;
        var invoices = new List<Invoice>
        {
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                InvoiceNumber = "INV-2025-00001",
                ClientId = clients[0].Id,
                IssueDate = now.AddDays(-25),
                DueDate = now.AddDays(-18),
                TotalAmount = 1250.00m,
                PaidAmount = 1250.00m,
                PendingAmount = 0,
                Status = InvoiceStatus.Paid,
                Notes = "Bulk order - 500 trays",
                CreatedAt = now.AddDays(-25)
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222202"),
                InvoiceNumber = "INV-2025-00002",
                ClientId = clients[0].Id,
                IssueDate = now.AddDays(-10),
                DueDate = now.AddDays(-3),
                TotalAmount = 890.50m,
                PaidAmount = 400.00m,
                PendingAmount = 490.50m,
                Status = InvoiceStatus.Overdue,
                Notes = null,
                CreatedAt = now.AddDays(-10)
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222203"),
                InvoiceNumber = "INV-2025-00003",
                ClientId = clients[1].Id,
                IssueDate = now.AddDays(-5),
                DueDate = now.AddDays(5),
                TotalAmount = 450.00m,
                PaidAmount = 0,
                PendingAmount = 450.00m,
                Status = InvoiceStatus.Pending,
                Notes = "Weekly delivery",
                CreatedAt = now.AddDays(-5)
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222204"),
                InvoiceNumber = "INV-2025-00004",
                ClientId = clients[2].Id,
                IssueDate = now.AddDays(-2),
                DueDate = now.AddDays(8),
                TotalAmount = 2100.00m,
                PaidAmount = 0,
                PendingAmount = 2100.00m,
                Status = InvoiceStatus.Pending,
                Notes = "Large order - 1000 trays",
                CreatedAt = now.AddDays(-2)
            }
        };

        var items = new List<InvoiceItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoices[0].Id,
                Description = "White eggs - Large (30 count tray)",
                ProductCode = "EGG-WL-30",
                Quantity = 100,
                Unit = "tray",
                UnitPrice = 8.50m
            },
            new()
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoices[0].Id,
                Description = "Brown eggs - Medium (24 count tray)",
                ProductCode = "EGG-BM-24",
                Quantity = 50,
                Unit = "tray",
                UnitPrice = 8.10m
            },
            new()
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoices[1].Id,
                Description = "White eggs - Large (30 count tray)",
                ProductCode = "EGG-WL-30",
                Quantity = 70,
                Unit = "tray",
                UnitPrice = 8.50m
            },
            new()
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoices[1].Id,
                Description = "Organic eggs - Free range (12 count)",
                ProductCode = "EGG-OR-12",
                Quantity = 30,
                Unit = "carton",
                UnitPrice = 9.35m
            },
            new()
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoices[2].Id,
                Description = "White eggs - Large (30 count tray)",
                ProductCode = "EGG-WL-30",
                Quantity = 50,
                Unit = "tray",
                UnitPrice = 9.00m
            },
            new()
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoices[3].Id,
                Description = "White eggs - Large (30 count tray)",
                ProductCode = "EGG-WL-30",
                Quantity = 200,
                Unit = "tray",
                UnitPrice = 8.50m
            },
            new()
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoices[3].Id,
                Description = "Brown eggs - Large (30 count tray)",
                ProductCode = "EGG-BL-30",
                Quantity = 50,
                Unit = "tray",
                UnitPrice = 9.00m
            }
        };

        var payments = new List<Payment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoices[0].Id,
                Amount = 1250.00m,
                PaymentDate = now.AddDays(-20),
                Reference = "CHQ-001",
                Notes = "Full payment",
                CreatedAt = now.AddDays(-20)
            },
            new()
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoices[1].Id,
                Amount = 400.00m,
                PaymentDate = now.AddDays(-5),
                Reference = "CHQ-002",
                Notes = "Partial payment",
                CreatedAt = now.AddDays(-5)
            }
        };

        context.Invoices.AddRange(invoices);
        context.InvoiceItems.AddRange(items);
        context.Payments.AddRange(payments);
        await context.SaveChangesAsync(ct);
    }
}

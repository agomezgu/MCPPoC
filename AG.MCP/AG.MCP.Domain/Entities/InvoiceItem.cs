using System.ComponentModel.DataAnnotations.Schema;

namespace AG.MCP.Domain.Entities;

public class InvoiceItem
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "unit";
    public decimal UnitPrice { get; set; }

    [NotMapped]
    public decimal LineTotal => Quantity * UnitPrice;

    public Invoice Invoice { get; set; } = null!;
}

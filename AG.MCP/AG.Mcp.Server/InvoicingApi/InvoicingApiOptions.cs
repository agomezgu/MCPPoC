namespace AG.Mcp.Server.InvoicingApi;

public class InvoicingApiOptions
{
    public const string SectionName = "AbceggApi";
    
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}

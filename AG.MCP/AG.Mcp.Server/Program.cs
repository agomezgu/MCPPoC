using Azure.Core;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Storage.Blobs;
using AG.Mcp.Server.Documents;
using AG.Mcp.Server.InvoicingApi;

// Critical for stdio transport: MCP protocol uses stdout for JSON-RPC. Any other output
// (e.g. paths, logs, .NET runtime) to stdout corrupts the stream and causes "not valid JSON" errors.
// Redirect Console.Out so errant writes go to stderr; MCP transport uses the raw stdout stream.
Console.SetOut(Console.Error);

var builder = WebApplication.CreateBuilder(args);


// Configure JSON, user secrets, and environment variables
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithHttpTransport(options => {
        options.Stateless = true;
        
    })
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly()
    .WithResourcesFromAssembly()
    
    ;

// Configure Azure clients and services
TokenCredential azureCredential;

if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
{
    // In production/staging, prefer user-assigned managed identity if configured
    // (Azure:ClientId is expected to be the user-assigned client id).
    var clientId = builder.Configuration["Azure:ClientId"];
    azureCredential = string.IsNullOrWhiteSpace(clientId)
        ? new DefaultAzureCredential()
        : new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(clientId));
}
else
{
    // local development environment
    azureCredential = new DefaultAzureCredential();
}

builder.Services.AddSingleton(_ => new SearchClient(
    new Uri(builder.Configuration["AzureSearch:Endpoint"]!),
    builder.Configuration["AzureSearch:IndexName"]!,
    azureCredential));
var hrmBlobServiceConnectionString = builder.Configuration["HRM_BLOB_SERVICE_CONNECTIONSTRING"];

    if (hrmBlobServiceConnectionString is not null)
    {
        builder.Services
            .AddSingleton(_ => new BlobServiceClient(hrmBlobServiceConnectionString));
    } 
    else 
    {
        builder.Services
            .AddSingleton(_ => new BlobServiceClient(
                new Uri(builder.Configuration["HRM_BLOB_SERVICE_URI"]!),
                azureCredential));
    }

builder.Services.AddSingleton<IHrmDocumentService, HrmDocumentService>();

builder.Services.Configure<InvoicingApiOptions>(builder.Configuration.GetSection(InvoicingApiOptions.SectionName));
builder.Services.AddHttpClient<IInvoicingApiClient, InvoicingApiClient>();

builder.Logging.AddConsole(options => 
    options.LogToStandardErrorThreshold = LogLevel.Trace
);
var app = builder.Build();

app.MapMcp("/mcp");

await app.RunAsync();
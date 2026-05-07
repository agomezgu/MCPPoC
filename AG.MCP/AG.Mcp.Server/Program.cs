using Azure.Core;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Storage.Blobs;
using AG.Mcp.Server.Documents;
using AG.Mcp.Server.InvoicingApi;
using AG.Mcp.Server.Ui;
using Microsoft.Extensions.FileProviders;
using ModelContextProtocol.Server;
using System.Text.Json.Nodes;

// Critical for stdio transport: MCP protocol uses stdout for JSON-RPC. Any other output
// (e.g. paths, logs, .NET runtime) to stdout corrupts the stream and causes "not valid JSON" errors.
// Redirect Console.Out so errant writes go to stderr; MCP transport uses the raw stdout stream.
Console.SetOut(Console.Error);

var builder = WebApplication.CreateBuilder(args);

var clientsUiTool = McpServerTool.Create(
    UiTools.OpenClientsUi,
    new McpServerToolCreateOptions
    {
        Name = "open_clients_ui",
        Title = "Open Clients UI",
        Description = "Open the local Angular clients app to create and list clients.",
        Meta = new JsonObject
        {
            ["ui"] = new JsonObject
            {
                ["resourceUri"] = UiResources.ClientsAppResourceUri
            },
            // Keep the legacy key for hosts that still rely on it.
            ["ui/resourceUri"] = UiResources.ClientsAppResourceUri
        }
    });


// Configure JSON, user secrets, and environment variables
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithHttpTransport(options => {
        // Sampling + elicitation require server->client requests, which are not
        // available in stateless mode.
        options.Stateless = false;
        
    })
    .WithTools([clientsUiTool])
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

var clientsUiPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "clients-ui");
Directory.CreateDirectory(clientsUiPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(clientsUiPath),
    RequestPath = "/clients-ui",
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.AccessControlAllowOrigin = "*";
        context.Context.Response.Headers.AccessControlAllowMethods = "GET, OPTIONS";
        context.Context.Response.Headers.AccessControlAllowHeaders = "Content-Type";
    }
});

app.MapMcp("/mcp");

await app.RunAsync();
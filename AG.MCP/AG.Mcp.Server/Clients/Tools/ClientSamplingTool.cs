using System.ComponentModel;
using System.Text.Json;
using AG.Mcp.Server.InvoicingApi.Models;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AG.Mcp.Server.Clients.Tools;

[McpServerToolType]
public static class ClientSamplingTool
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    [McpServerTool(Name = "sample_create_client_action", Title = "Sample: Convert Text to Create Client Action"),
     Description(
        "Demonstrates MCP Sampling by asking the connected client LLM to extract client details from natural language " +
        "and return a structured action for the default chat client-creation tool, create_client_elicit.")]
    public static async Task<CreateClientAction> SampleCreateClientAction(
        McpServer server,
        [Description("Natural language request that includes the client information to create.")] string message,
        CancellationToken cancellationToken)
    {
        if (server.ClientCapabilities?.Sampling is null)
        {
            throw new McpException("Client does not support sampling. Configure a client SamplingHandler/capability and retry.");
        }

        var result = await server.SampleAsync(
            new CreateMessageRequestParams
            {
                Messages =
                [
                    new SamplingMessage
                    {
                        Role = Role.User,
                        Content =
                        [
                            new TextContentBlock
                            {
                                Text = """
                                    Convert the user's natural language message into a create-client action.

                                    Return ONLY valid JSON with this exact shape:
                                    {
                                      "action": "create_client",
                                      "toolName": "create_client_elicit",
                                      "request": {
                                        "name": "client display name",
                                        "taxId": "tax identification number",
                                        "email": "contact email or null",
                                        "phone": "contact phone or null",
                                        "address": "postal address or null"
                                      }
                                    }

                                    Rules:
                                    - Only return action "create_client" when the user is asking to create/onboard/register/add a client.
                                    - Extract name and taxId when they are present in the message.
                                    - Use null for optional email, phone, or address when not present.
                                    - If the message is not a client creation request, return:
                                      {"action":"none","toolName":null,"request":null,"reason":"not a client creation request"}
                                    - If name or taxId is missing, return:
                                      {"action":"none","toolName":null,"request":null,"reason":"missing required name or taxId"}
                                    """
                            },
                            new TextContentBlock { Text = message }
                        ]
                    }
                ],
                MaxTokens = 512,
            },
            cancellationToken);

        var responseText = result.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(responseText))
        {
            throw new McpException("No create-client action was returned by sampling.");
        }

        var action = JsonSerializer.Deserialize<CreateClientAction>(
            ExtractJsonObject(responseText),
            JsonOptions);

        if (action?.Request is null || !string.Equals(action.Action, "create_client", StringComparison.OrdinalIgnoreCase))
        {
            throw new McpException(action?.Reason ?? "The message could not be converted into a create-client action.");
        }

        var request = action.Request;
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.TaxId))
        {
            throw new McpException("The message must include both client name and tax ID to create a client action.");
        }

        return action with
        {
            Action = "create_client",
            ToolName = "create_client_elicit",
            Request = new CreateClientRequest(
                request.Name.Trim(),
                request.TaxId.Trim(),
                NormalizeOptional(request.Email),
                NormalizeOptional(request.Phone),
                NormalizeOptional(request.Address)),
            Reason = null
        };
    }

    private static string ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');

        if (start < 0 || end <= start)
        {
            throw new McpException("Sampling response did not contain a JSON object.");
        }

        return text[start..(end + 1)];
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public sealed record CreateClientAction(
        string Action,
        string? ToolName,
        CreateClientRequest? Request,
        string? Reason = null);
}

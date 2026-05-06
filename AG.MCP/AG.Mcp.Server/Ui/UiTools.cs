using System.ComponentModel;

namespace AG.Mcp.Server.Ui;

public static class UiTools
{
    [Description("Opens the AG MCP clients UI app where users can create and review clients.")]
    public static string OpenClientsUi()
        => "Open the Clients UI to create clients or review the current list.";
}

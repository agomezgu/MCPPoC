param(
    [switch]$KillExisting
)

$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$apiProject = Join-Path $root 'AG.MCP.Api\AG.MCP.Api.csproj'
$mcpProject = Join-Path $root 'AG.Mcp.Server\AG.Mcp.Server.csproj'
$uiPath = Join-Path $root 'AG.MCP.Ui'

function Stop-ProcessOnPort {
    param(
        [Parameter(Mandatory)]
        [int]$Port
    )

    $connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    $processIds = $connections | Select-Object -ExpandProperty OwningProcess -Unique

    foreach ($processId in $processIds) {
        if ($processId -and $processId -ne $PID) {
            Write-Host "Stopping process $processId listening on port $Port..."
            Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
        }
    }
}

function Start-CmdConsole {
    param(
        [Parameter(Mandatory)]
        [string]$Title,

        [Parameter(Mandatory)]
        [string]$WorkingDirectory,

        [Parameter(Mandatory)]
        [string]$Command
    )

    $cmd = "title $Title && cd /d `"$WorkingDirectory`" && $Command"
    Start-Process -FilePath 'cmd.exe' -ArgumentList @('/k', $cmd)
}

if ($KillExisting) {
    4200, 44328, 7280, 5280, 57656, 57658, 7070, 6274, 6277 | ForEach-Object {
        Stop-ProcessOnPort -Port $_
    }
}

Write-Host 'Building Angular UI for MCP server hosting...'
Push-Location $uiPath
try {
    npm run build:mcp
}
finally {
    Pop-Location
}

Start-CmdConsole `
    -Title 'AG MCP API' `
    -WorkingDirectory $root `
    -Command "set ASPNETCORE_ENVIRONMENT=Development && dotnet run --configuration Release --project `"$apiProject`" --urls `"https://localhost:44328;https://localhost:7280;`""

Start-CmdConsole `
    -Title 'AG MCP Server' `
    -WorkingDirectory $root `
    -Command "set ASPNETCORE_ENVIRONMENT=Development && dotnet run --configuration Release --project `"$mcpProject`" --launch-profile AG.Mcp.Server"

Start-CmdConsole `
    -Title 'AG MCP Inspector' `
    -WorkingDirectory (Split-Path $mcpProject -Parent) `
    -Command "set ASPNETCORE_ENVIRONMENT=Development && npx @modelcontextprotocol/inspector dotnet run --configuration Release --project `"$mcpProject`" --urls http://localhost:7070 --no-launch-profile"

Start-CmdConsole `
    -Title 'AG MCP UI' `
    -WorkingDirectory $uiPath `
    -Command 'npm start'

Write-Host 'Started AG MCP API, MCP Server, MCP Inspector, and UI in separate cmd consoles.'

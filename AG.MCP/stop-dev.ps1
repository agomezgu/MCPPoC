<#
.SYNOPSIS
    Stops any local processes listening on the ports used by start-dev.ps1.

.DESCRIPTION
    Mirrors the port list from start-dev.ps1 and force-stops any listening
    process bound to those ports. Useful for clearing stale dotnet, npm,
    or inspector processes before re-running start-dev.ps1.

.EXAMPLE
    PS> .\stop-dev.ps1

.EXAMPLE
    PS> .\stop-dev.ps1 -Ports 4200,7070
#>
param(
    [int[]]$Ports = @(4200, 44328, 7280, 5280, 57656, 57658, 7070, 6274, 6277)
)

$ErrorActionPreference = 'Stop'

function Stop-ProcessOnPort {
    param(
        [Parameter(Mandatory)]
        [int]$Port
    )

    $connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    $processIds = $connections | Select-Object -ExpandProperty OwningProcess -Unique

    if (-not $processIds) {
        Write-Host "Port $Port : no listening process found."
        return
    }

    foreach ($processId in $processIds) {
        if (-not $processId -or $processId -eq $PID) { continue }

        $proc = Get-Process -Id $processId -ErrorAction SilentlyContinue
        $name = if ($proc) { $proc.ProcessName } else { '<unknown>' }

        Write-Host "Port $Port : stopping process $processId ($name)..."
        Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Releasing ports used by start-dev.ps1: $($Ports -join ', ')"
foreach ($port in $Ports) {
    Stop-ProcessOnPort -Port $port
}

Write-Host 'Done. All matching processes have been signalled to stop.'

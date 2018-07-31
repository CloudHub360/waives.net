#Requires -Version 5.0

$global:ErrorActionPreference = "Stop"
$global:ProgressPreference = 'SilentlyContinue' # Hide progress bars

$env:PATH += "$([Io.Path]::PathSeparator)$($env:HOME)/.dotnet"

function global:RestoreBuildLevelPackages() {
  try {
    Push-Location "$PsScriptRoot\.."

    Write-Host "Testing for InvokeBuild..."
    if ($null -eq (Get-Module -ListAvailable InvokeBuild)) {
      Write-Host "Installing InvokeBuild..."
      Install-Module -Force InvokeBuild -Scope CurrentUser
      Get-Command -Module InvokeBuild # This seems to set the necessary aliases?!
    }

    Write-Host "Testing for dotnet"
    if (!(Get-Command dotnet)) {
      Write-Host "Installing dotnet CLI..."
      # Install dotnet CLI if required, taking the required version from global.json
      $dotnetInstallScript = "${pwd}\dotnet-install.ps1"
      Invoke-WebRequest `
        -Uri "https://dot.net/v1/dotnet-install.ps1" `
        -OutFile $dotnetInstallScript

      & $dotnetInstallScript
      Remove-Item $dotnetInstallScript

      # Add dotnet to the path, as the installation script doesn't do this reliably
      $env:path += ";$($env:LOCALAPPDATA)\Microsoft\dotnet\"

      # Do the initial package population, etc.
      dotnet
    }
  } finally {
    Pop-Location
  }
}

<#
.SYNOPSIS
Build.

.DESCRIPTION
This is really a wrapper around build.ps1 (build.ps1 is our actual build script.
2 main steps:

    1 - Restore build-time dependencies via paket.
    2 - Execute the build. (psake build.ps1)

In theory, Teamcity will also use this build command. Probably like this:
`build -Task Build`

.EXAMPLE
build
Run the build script with default values for all parameters

.EXAMPLE
build -Task Clean
Run the build script and execute only the 'Clean' task.
#>
function global:build() {
  [CmdletBinding()]
  param(
      # The Tasks to execute. An empty list runs the default task, as defined in build.ps1
      [Parameter(Position=0)]
      [string[]] $Tasks = @(),

      [Parameter(Position=1)]
      [string] $SolutionFile = "Waives.NET.sln",

      [Parameter(Position=3)]
      [string] $Configuration = "Release",

      [Parameter(Position=4)]
      [string] $BuildNumber = '0'
  )

  RestoreBuildLevelPackages

  Invoke-Build `
      -File "build\build.ps1" `
      -SolutionFile (Resolve-Path "$SolutionFile") `
      -Configuration "$Configuration" `
      -Task $Tasks
}

Write-Host "This is the Waives.NET repo. And here are the available commands:" -Fore Magenta
Write-Host "`t build" -Fore Green
Write-Host "For more information about the commands, use Get-Help <command-name>" -Fore Magenta
Write-Host "To view the tasks exposed by each command, use <command-name> help" -Fore Magenta

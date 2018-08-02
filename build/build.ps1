param(
  [string]$Configuration = "Release",
  [string]$SolutionFile = "Waives.NET.sln",
  [string]$buildDir = "bin"
)

$RootDir = "$PsScriptRoot/.."

function Get-Projects([string]$SolutionFile) {
  $SOLUTION_FOLDER_GUID = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}"

  if (Test-ProjectFile $SolutionFile) {
    return New-Object PSObject -Property @{
      Name = [Io.Path]::GetFileNameWithoutExtension(($SolutionFile))
      File = [Io.FileInfo]::new((Resolve-Path $SolutionFile))
    }
  }

  return Get-Content $SolutionFile |
    Select-String 'Project\(' |
    Select-String $SOLUTION_FOLDER_GUID -NotMatch | ForEach-Object {
    $projectParts = $_ -Split "[,=]" | ForEach-Object {
      # Split by tokens "," and "="
      $_.Trim('[ "{}]') # trim spaces, quotations and braces
    }
    New-Object PSObject -Property @{
      Name = $projectParts[1]
      File = [Io.FileInfo]::new((Resolve-Path $projectParts[2]))
    }
  }
}

function Test-ProjectFile([Io.FileInfo]$projectFile) {
  return (Get-Content $projectFile)[0].StartsWith('<Project Sdk="Microsoft.NET.Sdk')
}

Task Build {
  try {
    Push-Location $RootDir

    [int] $buildNumber = $env:BUILD_NUMBER
    if ($buildNumber -eq $null) { $buildNumber = 0 }

    $isMaster = ($(git rev-parse --abbrev-ref HEAD) -eq 'master')

    if ($isMaster) {
      # Build stable package versions
      $versionSuffix = ''
    } else {
      # Build a pre-release package on branches
      $versionSuffix = "$([string]::Format('pre-{0:d6}', $buildNumber))"
    }

    dotnet build $SolutionFile -c $Configuration --version-suffix=$versionSuffix
  } finally {
    Pop-Location
  }
}

Task Test {
  try {
    Push-Location $RootDir
    # dotnet test doesn't properly handle solutions files, expecting a project file
    # Enumerate the test projects in the solution and call dotnet test on each one
    $tests = Get-Projects $SolutionFile |? { $_.Name -like "*.Tests" }
    $tests |% {
      Push-Location $_.File.Directory
      Write-Information "Running tests from $($_.Name)"
       exec { dotnet test --configuration $Configuration --no-build }
    }
  } finally {
    Pop-Location
  }
}

Task . Build
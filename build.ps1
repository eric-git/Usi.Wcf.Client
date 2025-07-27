<#
.SYNOPSIS
  Build script for Usi.Wcf.Client solution.
.DESCRIPTION
  This script is used to clean and build the Usi.Wcf.Client solution.
.PARAMETER clean
  Clean the build artifacts.
.PARAMETER build
  Build the solution. This forces a restore of the solution.
.EXAMPLE
  ./build.ps1 -clean
  Cleans the build artifacts.
.EXAMPLE
  ./build.ps1 -build
  Builds the solution.
.EXAMPLE
  ./build.ps1 -clean -build
  Cleans the build artifacts and then builds the solution.
.EXAMPLE
  ./build.ps1
  Displays an error message indicating that no action was specified.
#>
[CmdletBinding()]
param(
  [Parameter(HelpMessage = "Clean the build artifacts.")]
  [switch]
  $clean,
  [Parameter(HelpMessage = "Build the solution. This forces a restore of the solution.")]
  [switch]
  $build
)
begin {
  Write-Host "Starting build script..."
  $ErrorActionPreference = "Stop"
  $InformationPreference = "Continue"
  if (-not $clean -and -not $build) {
    Write-Error "No action specified. Use -clean to clean and/or -build to build."
  }
  $solutionPath = "./src/Usi.Wcf.Client.sln"
}
process {
  if ($clean) {
    Write-Host "Cleaning build artifacts..."
    dotnet clean $solutionPath --nologo
  }
  if ($build) {
    Write-Host "Building project..."
    dotnet build $solutionPath --nologo --force
  }
}
end {
  Write-Host "Build script completed."
}

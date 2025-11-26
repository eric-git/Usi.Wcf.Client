# -----------------------------------------------------------------------------
# SYNOPSIS
#   Build script for Usi.Wcf.Client solution.
# DESCRIPTION
#   This script is used to clean and build the Usi.Wcf.Client solution.
# PARAMETERS
#   --clean   Clean the build artifacts.
#   --build   Build the solution (forces restore).
# EXAMPLES
#   ./build.sh --clean
#   ./build.sh --build
#   ./build.sh --clean --build
#   ./build.sh   # error: no action specified
# -----------------------------------------------------------------------------
#!/usr/bin/env bash
set -euo pipefail

solutionPath="./src/Usi.Wcf.Client.sln"
clean=false
build=false
for arg in "$@"; do
  case "$arg" in
    --clean) clean=true ;;
    --build) build=true ;;
    *) echo "Unknown option: $arg" >&2; exit 1 ;;
  esac
done

echo "Starting build script..."
if [ "$clean" = false ] && [ "$build" = false ]; then
  echo "Error: No action specified. Use --clean and/or --build." >&2
  exit 1
fi
if [ "$clean" = true ]; then
  echo "Cleaning build artifacts..."
  dotnet clean "$solutionPath" --nologo
fi
if [ "$build" = true ]; then
  echo "Building project..."
  dotnet build "$solutionPath" --nologo --force
fi
echo "Build script completed."
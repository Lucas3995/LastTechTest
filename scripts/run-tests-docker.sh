#!/usr/bin/env bash
# Run backend tests inside a .NET 10 SDK container (no local SDK 10 required).
set -e
cd "$(dirname "$0")/.."
docker run --rm \
  -v "$(pwd):/src" \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  bash -c "dotnet restore backend/LastTechTest.sln && dotnet build backend/LastTechTest.sln -c Release --no-restore && dotnet test backend/LastTechTest.Testes/LastTechTest.Testes.csproj -c Release --no-build"

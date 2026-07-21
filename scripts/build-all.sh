#!/usr/bin/env bash
# build-all.sh — restaura y compila TODA la solucion en Release, cero warnings.
set -euo pipefail

RAIZ="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SLN="$RAIZ/SAI.Service.Core.sln"

echo "==> restore"
dotnet restore "$SLN"

echo "==> build (Release) solucion completa"
dotnet build "$SLN" --configuration Release --no-restore

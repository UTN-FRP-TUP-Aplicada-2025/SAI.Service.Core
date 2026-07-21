#!/usr/bin/env bash
# build.sh <proyecto> — compila UN proyecto en configuracion Release, cero warnings.
# Agnostico al entorno: asume dotnet en el PATH (vive en el Dev Container).
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "uso: $0 <proyecto>   (ej: $0 SAI.Service.Core.Web)" >&2
  exit 2
fi

PROYECTO="$1"
RAIZ="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Busca el .csproj por nombre bajo src/ y tests/.
CSPROJ="$(find "$RAIZ/src" "$RAIZ/tests" -name "${PROYECTO}.csproj" | head -n 1)"
if [[ -z "${CSPROJ}" ]]; then
  echo "no se encontro ${PROYECTO}.csproj bajo src/ ni tests/" >&2
  exit 1
fi

echo "==> build (Release) ${PROYECTO}"
dotnet build "${CSPROJ}" --configuration Release

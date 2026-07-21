#!/usr/bin/env bash
# run.sh <proyecto> — corre UN proyecto (tipicamente SAI.Service.Core.Web).
# NO es para depurar: la depuracion va por .vscode/launch.json con F5 (§17.P.8).
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "uso: $0 <proyecto>   (ej: $0 SAI.Service.Core.Web)" >&2
  exit 2
fi

PROYECTO="$1"
RAIZ="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

CSPROJ="$(find "$RAIZ/src" "$RAIZ/tests" -name "${PROYECTO}.csproj" | head -n 1)"
if [[ -z "${CSPROJ}" ]]; then
  echo "no se encontro ${PROYECTO}.csproj bajo src/ ni tests/" >&2
  exit 1
fi

# Development habilita el endpoint HTTPS 8443 con el certificado de desarrollo
# (dotnet dev-certs https --trust la primera vez) y el HTTP 8080.
export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"

echo "==> run ${PROYECTO}  (HTTP :8080  HTTPS :8443)"
dotnet run --project "${CSPROJ}"

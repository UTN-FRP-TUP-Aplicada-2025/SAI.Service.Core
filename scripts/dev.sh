#!/usr/bin/env bash
# dev.sh — (opcional) levanta el Dev Container y compila todo dentro de el.
# Requiere el CLI de devcontainers (npm i -g @devcontainers/cli) y Docker en el host.
set -euo pipefail

RAIZ="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

if ! command -v devcontainer >/dev/null 2>&1; then
  echo "no se encontro el CLI 'devcontainer'. En VS Code use 'Reopen in Container'." >&2
  echo "o instale: npm i -g @devcontainers/cli" >&2
  exit 1
fi

echo "==> devcontainer up"
devcontainer up --workspace-folder "$RAIZ"

echo "==> build-all dentro del contenedor"
devcontainer exec --workspace-folder "$RAIZ" ./scripts/build-all.sh

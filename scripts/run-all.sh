#!/usr/bin/env bash
# run-all.sh — corre el servicio (el host es el unico proceso: SAI.Service.Core.Web).
# Es un web-monolith: "toda la solucion" se corre arrancando el host.
set -euo pipefail

RAIZ="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
exec "$RAIZ/scripts/run.sh" SAI.Service.Core.Web

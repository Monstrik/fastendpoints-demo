#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
REPORT_DIR="$ROOT_DIR/security/reports/$(date +%Y%m%d-%H%M%S)"
mkdir -p "$REPORT_DIR"

echo "Security review artifacts will be stored in: $REPORT_DIR"

(
  cd "$ROOT_DIR"
  dotnet list src/MyWebAppFastEndpoints/MyWebAppFastEndpoints.csproj package --vulnerable --include-transitive
) | tee "$REPORT_DIR/dotnet-vulnerable.txt"

(
  cd "$ROOT_DIR"
  dotnet list src/MyWebAppFastEndpoints/MyWebAppFastEndpoints.csproj package --outdated
) | tee "$REPORT_DIR/dotnet-outdated.txt"

if command -v pnpm >/dev/null 2>&1; then
  (
    cd "$ROOT_DIR/../frontend"
    pnpm audit --prod --json
  ) > "$REPORT_DIR/frontend-pnpm-audit.json" || true
else
  echo "pnpm not found; skipping frontend audit" | tee "$REPORT_DIR/frontend-pnpm-audit.txt"
fi

if [[ "${RUN_AUTHZ_SMOKE:-1}" == "1" ]]; then
  "$ROOT_DIR/security/authz-smoke.sh" | tee "$REPORT_DIR/authz-smoke.txt"
fi

echo "Security review run complete. Check: $REPORT_DIR"


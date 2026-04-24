#!/usr/bin/env bash
set -euo pipefail

if ! command -v k6 >/dev/null 2>&1; then
  echo "k6 is required. Install with: brew install k6"
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
RESULTS_DIR="$ROOT_DIR/perf/results/$(date +%Y%m%d-%H%M%S)"
mkdir -p "$RESULTS_DIR"

BASE_URL="${BASE_URL:-http://localhost:5116}"

echo "Running performance tests against $BASE_URL"
echo "Results: $RESULTS_DIR"

k6 run \
  -e BASE_URL="$BASE_URL" \
  "$ROOT_DIR/perf/k6/public-read.js" \
  --summary-export "$RESULTS_DIR/public-read-summary.json" | tee "$RESULTS_DIR/public-read.log"

k6 run \
  -e BASE_URL="$BASE_URL" \
  -e ADMIN_LOGIN="${ADMIN_LOGIN:-admin}" \
  -e ADMIN_PASSWORD="${ADMIN_PASSWORD:-Admin123!}" \
  "$ROOT_DIR/perf/k6/auth-write.js" \
  --summary-export "$RESULTS_DIR/auth-write-summary.json" | tee "$RESULTS_DIR/auth-write.log"

echo "Done. Review JSON summaries and logs in: $RESULTS_DIR"


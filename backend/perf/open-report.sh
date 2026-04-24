#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
RESULTS_ROOT="$ROOT_DIR/perf/results"

if [[ ! -d "$RESULTS_ROOT" ]]; then
  echo "Results directory not found: $RESULTS_ROOT"
  echo "Run ./perf/run-perf.sh first."
  exit 1
fi

target_dir=""
if [[ $# -ge 1 ]]; then
  target_dir="$RESULTS_ROOT/$1"
  if [[ ! -d "$target_dir" ]]; then
    echo "Result folder does not exist: $target_dir"
    exit 1
  fi
else
  target_dir="$(find "$RESULTS_ROOT" -mindepth 1 -maxdepth 1 -type d | sort | tail -n 1)"
  if [[ -z "${target_dir:-}" ]]; then
    echo "No result folders found under: $RESULTS_ROOT"
    echo "Run ./perf/run-perf.sh first."
    exit 1
  fi
fi

echo "Opening: $target_dir"

for file in \
  "$target_dir/public-read-summary.json" \
  "$target_dir/auth-write-summary.json" \
  "$target_dir/public-read.log" \
  "$target_dir/auth-write.log"; do
  if [[ -f "$file" ]]; then
    open "$file"
  fi
done

echo "Done."


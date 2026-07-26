#!/usr/bin/env bash
# Generate test traffic for the OpenObserve demo.
# Repeatedly calls the demo endpoints so logs, traces and metrics flow into OpenObserve.
#
# Usage:
#   ./generate-traffic.sh                 # 10 bursts against http://localhost:5119
#   ./generate-traffic.sh 50              # 50 bursts
#   ./generate-traffic.sh 50 http://localhost:5119 30   # 50 bursts, custom URL, 30 requests/burst

set -euo pipefail

BURSTS="${1:-10}"
BASE_URL="${2:-http://localhost:5119}"
PER_BURST="${3:-20}"
SLEEP_SECONDS="${SLEEP_SECONDS:-1}"

echo "Target:        $BASE_URL"
echo "Bursts:        $BURSTS"
echo "Per burst:     $PER_BURST requests"
echo "Sleep between: ${SLEEP_SECONDS}s"
echo "------------------------------------------------------------"

for ((i = 1; i <= BURSTS; i++)); do
  # Hit the root page (custom span + counter + logs)
  curl -s -o /dev/null "$BASE_URL/" || true

  # Hit the traffic generator (burst of varied telemetry)
  resp="$(curl -s "$BASE_URL/generate-traffic?count=$PER_BURST" || echo '{}')"
  echo "burst $i/$BURSTS -> $resp"

  sleep "$SLEEP_SECONDS"
done

echo "------------------------------------------------------------"
echo "Done. OpenObserve: http://localhost:5080 (org: default, streams: dotnetlogs, dotnettracing, dotnetmetrics)"
echo "Tip: widen the time range if you see nothing — the default window is narrow."

#!/usr/bin/env bash
# Bulk test / and /hi endpoints with parallel or time-paced requests.
# Usage: ./bulk-test.sh 50                           # 50 requests, as fast as possible
#        ./bulk-test.sh 50 http://localhost:5119     # custom URL
#        ./bulk-test.sh 120 120                      # 120 requests over 120 seconds (1 req/s)
#        ./bulk-test.sh 120 http://localhost:5119 120 # custom URL, paced over 120s

set -euo pipefail

TOTAL="${1:-50}"
DURATION=0

if [[ "${2:-}" =~ ^http.* ]]; then
    BASE_URL="$2"
    DURATION="${3:-0}"
else
    DURATION="${2:-0}"
    BASE_URL="${3:-http://localhost:5119}"
fi

start_time=$SECONDS

echo "Target:   $BASE_URL"
echo "Total:    $TOTAL requests"
if [ "$DURATION" -gt 0 ] 2>/dev/null; then
    DELAY=$(echo "scale=3; $DURATION / $TOTAL" | bc 2>/dev/null || echo "1.0")
    echo "Pacing:   over ${DURATION}s (${DELAY}s between requests)"
else
    PARALLEL_MIN=3
    PARALLEL_MAX=10
    PARALLEL=$(( RANDOM % (PARALLEL_MAX - PARALLEL_MIN + 1) + PARALLEL_MIN ))
    echo "Parallel: $PARALLEL"
fi
echo "----------------------------------------"

success=0
fail=0

run_request() {
    local endpoint
    if (( RANDOM % 2 == 0 )); then
        endpoint="/"
    else
        endpoint="/hi"
    fi
    if curl -s -o /dev/null -w "%{http_code}" "$BASE_URL$endpoint" 2>/dev/null | grep -q '200\|500'; then
        echo -n "."
        return 0
    else
        echo -n "x"
        return 1
    fi
}

if [ "$DURATION" -gt 0 ] 2>/dev/null; then
    for ((i = 0; i < TOTAL; i++)); do
        run_request "$i"
        sleep "$DELAY"
    done
else
    export -f run_request
    export BASE_URL

    count=0
    while [ $count -lt "$TOTAL" ]; do
        remaining=$((TOTAL - count))
        batch=$(( remaining < PARALLEL ? remaining : PARALLEL ))

        seq 1 "$batch" | xargs -I{} -P "$batch" bash -c 'run_request "$1"' _ {}

        count=$(( count + batch ))
    done
fi

echo ""
elapsed=$(( SECONDS - start_time ))
echo "----------------------------------------"
echo "Done: $TOTAL requests in ${elapsed}s ($(( TOTAL * 1000 / (elapsed + 1) )) req/s approx)"

#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5289}"
DB_CONTAINER="${DB_CONTAINER:-inms-db}"
SQL_PASSWORD="${SQL_PASSWORD:-Iac@4336}"

pass() { echo "PASS: $1"; }
fail() { echo "FAIL: $1"; exit 1; }

api_get() {
  local path="$1"
  curl -sS -f "${BASE_URL}${path}"
}

api_post() {
  local path="$1"
  curl -sS -f -X POST "${BASE_URL}${path}"
}

sql_query() {
  local query="$1"
  docker exec -i "$DB_CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SQL_PASSWORD" -C -h -1 -W -Q "$query"
}

echo "Checking API availability at ${BASE_URL} ..."
api_get "/weatherforecast" >/dev/null || fail "API is not reachable"
pass "API reachable"

echo "Scenario 1: root failure on device 1"
api_post "/api/impact-analysis/analyze/1" >/dev/null || fail "Analyze device 1 failed"
result1="$(api_get "/api/impact-analysis/result/1")"

echo "$result1" | grep -q '"rootCause":null' && fail "Device 1 should have a root cause"
for id in 3 4 6 7; do
  echo "$result1" | grep -q "\"deviceId\":$id" || fail "Device $id missing from impacted list for root 1"
done
pass "Root detection and downstream impact for device 1"

echo "Scenario 2: non-root failure on child device 3"
api_post "/api/impact-analysis/analyze/3" >/dev/null || fail "Analyze device 3 failed"
result3="$(api_get "/api/impact-analysis/result/3")"
echo "$result3" | grep -q '"rootCause":null' || fail "Device 3 should not be marked as root when parent is down/impacted"
pass "Non-root detection for device 3"

echo "Scenario 3: clear root failure on device 1"
api_post "/api/impact-analysis/clear/1" >/dev/null || fail "Clear device 1 failed"

root_rows="$(sql_query "USE INMS_SLT; SELECT COUNT(*) FROM RootCause WHERE RootCauseDeviceId = 1;")"
impact_rows="$(sql_query "USE INMS_SLT; SELECT COUNT(*) FROM ImpactedDevice;")"

echo "$root_rows" | grep -q "0" || fail "Root cause for device 1 was not cleared"
echo "$impact_rows" | grep -q "0" || fail "Impacted device rows were not cleared"
pass "Recovery flow cleanup (root cause and impact rows)"

echo
echo "All impact-analysis demo checks passed."
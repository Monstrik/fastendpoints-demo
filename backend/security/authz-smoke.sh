#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5116}"
ADMIN_LOGIN="${ADMIN_LOGIN:-admin}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-Admin123!}"

echo "[authz] Base URL: $BASE_URL"

anon_users_status=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/users")
if [[ "$anon_users_status" != "401" ]]; then
  echo "[authz] FAIL: anonymous GET /api/users expected 401, got $anon_users_status"
  exit 1
fi

echo "[authz] PASS: anonymous GET /api/users returns 401"

login_body=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"login\":\"$ADMIN_LOGIN\",\"password\":\"$ADMIN_PASSWORD\"}")

token=$(printf "%s" "$login_body" | sed -n 's/.*"token":"\([^"]*\)".*/\1/p')
if [[ -z "$token" ]]; then
  echo "[authz] FAIL: could not retrieve admin token"
  echo "$login_body"
  exit 1
fi

admin_users_status=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/users" \
  -H "Authorization: Bearer $token")
if [[ "$admin_users_status" != "200" ]]; then
  echo "[authz] FAIL: admin GET /api/users expected 200, got $admin_users_status"
  exit 1
fi

echo "[authz] PASS: admin GET /api/users returns 200"

echo "[authz] Smoke checks passed"


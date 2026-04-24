# Backend Security Review

## Scope

- Backend API (`backend/src/MyWebAppFastEndpoints`)
- JWT authentication setup
- Role-protected endpoints
- Dependency vulnerability baseline

## How to Run

1. Start backend locally.
2. Run scripted checks:

```bash
cd backend
chmod +x security/run-security-review.sh security/authz-smoke.sh
./security/run-security-review.sh
```

Artifacts are written to `backend/security/reports/<timestamp>/`.

## Current Baseline (2026-04-24)

- Dependency CVE check (`nuget` via tool-assisted scan): **no known CVEs** found for current backend and test packages.
- Auth smoke checks (script):
  - anonymous `GET /api/users` -> expected `401`
  - admin `GET /api/users` with token -> expected `200`

## Manual Review Checklist

- [x] JWT signing key must come from environment (`Jwt__SigningKey`) and be at least 32 chars.
- [x] `.env` + `.env.local` precedence documented and implemented.
- [x] Post endpoints consistently map missing entities to `404` through a single null-return contract.
- [x] Password hashing uses dedicated hasher service.
- [ ] Add centralized rate limiting for auth endpoints (`/api/auth/login`, `/api/auth/forgot-password`).
- [ ] Add request/response security headers policy review (if API gateway does not inject them).
- [ ] Confirm secret rotation process and production secret store integration.

## Recommended Next Hardening Steps

1. Add ASP.NET rate limiting middleware for login and forgot-password endpoints.
2. Add token revocation/blacklist strategy if immediate logout invalidation is required.
3. Add CI job that runs vulnerability and authz smoke checks on every PR.


# Backend Performance / Load Tests

This folder contains a lightweight k6 harness for backend API load baselining.

## Scenarios

- `k6/public-read.js`
  - hits `/api/public/posts` and `/api/public/users`
  - default: 20 VUs for 30s
- `k6/auth-write.js`
  - logs in via `/api/auth/login`
  - creates posts via `/api/posts`
  - default: 10 VUs for 30s

## Prerequisites

```bash
brew install k6
```

Start backend before running load tests:

```bash
cd backend
cp .env.example .env
# set Jwt__SigningKey in .env (>= 32 chars)
dotnet run --project src/MyWebAppFastEndpoints/MyWebAppFastEndpoints.csproj
```

## Run

```bash
cd backend
chmod +x perf/run-perf.sh
./perf/run-perf.sh
```

## Live Dashboard (k6 Web UI)

For local development, you can run a single scenario with the built-in k6 web dashboard:

```bash
cd backend
export K6_WEB_DASHBOARD=true
export K6_WEB_DASHBOARD_OPEN=true
k6 run perf/k6/auth-write.js
```

Public-read scenario with the same dashboard:

```bash
cd backend
export K6_WEB_DASHBOARD=true
export K6_WEB_DASHBOARD_OPEN=true
k6 run perf/k6/public-read.js
```

This opens a local browser dashboard while the test is running.

Optional env overrides:

```bash
BASE_URL=http://localhost:5116 \
ADMIN_LOGIN=admin \
ADMIN_PASSWORD='Admin123!' \
./perf/run-perf.sh
```

Artifacts are written to `backend/perf/results/<timestamp>/`.

## Open Results (macOS)

Open the latest run artifacts (JSON summaries + logs):

```bash
cd backend
chmod +x perf/open-report.sh
./perf/open-report.sh
```

Open a specific timestamped run:

```bash
cd backend
./perf/open-report.sh 20260424-140111
```


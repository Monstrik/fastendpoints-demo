# Backend Tests

## Running

```bash
# Run all tests
dotnet test backend/MyWebAppFastEndpoints.sln

# Run with coverage (requires coverlet.collector)
dotnet test backend/MyWebAppFastEndpoints.sln --collect:"XPlat Code Coverage"

# Generate HTML report (requires reportgenerator tool)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"backend/**/TestResults/**/coverage.cobertura.xml" -targetdir:"backend/coverage-report" -reporttypes:Html

# Open report
open backend/coverage-report/index.html
```

## Performance / Load

```bash
# Requires k6 (macOS)
brew install k6

# Start backend in another terminal, then run load scenarios
cd backend
chmod +x perf/run-perf.sh
./perf/run-perf.sh
```

Results are saved under `backend/perf/results/<timestamp>/`.

## Security Review

```bash
cd backend
chmod +x security/run-security-review.sh security/authz-smoke.sh
./security/run-security-review.sh
```

Review findings in `backend/security/SECURITY_REVIEW.md` and generated artifacts in `backend/security/reports/<timestamp>/`.

## Coverage

- **User Management** — create, read, update, delete, login validation, status
- **Post Management** — create, retrieve (all / public / by author), hide/unhide
- **Reactions** — like/dislike, count aggregation, viewer tracking, reaction switching
- **Security** — PBKDF2-SHA256 hashing, JWT auth, admin vs user authorization
- **Edge Cases** — empty input, malformed data, special chars, case sensitivity, concurrency

## Dependencies

- `Microsoft.EntityFrameworkCore.InMemory` — in-memory database for unit tests
- `Microsoft.AspNetCore.Mvc.Testing` — `WebApplicationFactory` for integration tests

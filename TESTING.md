# Backend Tests

## Running

```bash
# Run all tests
dotnet test

# Run with coverage (requires coverlet.collector)
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report (requires reportgenerator tool)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"tests/**/TestResults/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html

# Open report
open coverage-report/index.html
```

## Coverage

- **User Management** — create, read, update, delete, login validation, status
- **Post Management** — create, retrieve (all / public / by author), hide/unhide
- **Reactions** — like/dislike, count aggregation, viewer tracking, reaction switching
- **Security** — PBKDF2-SHA256 hashing, JWT auth, admin vs user authorization
- **Edge Cases** — empty input, malformed data, special chars, case sensitivity, concurrency

## Dependencies

- `Microsoft.EntityFrameworkCore.InMemory` — in-memory database for unit tests
- `Microsoft.AspNetCore.Mvc.Testing` — `WebApplicationFactory` for integration tests

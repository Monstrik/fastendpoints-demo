# Backend - MyWebAppFastEndpoints

This is the FastEndpoints API for the MyWebApp demo project, built with .NET 8.

## Features

- **User Management**: Registration, login, and profile management.
- **Post Management**: Creating, viewing, and managing posts.
- **Reactions**: Like and dislike functionality on posts.
- **Security**: JWT-based authentication and PBKDF2-SHA256 password hashing.
- **Database**: Entity Framework Core with SQLite.

## Technologies

- **Runtime**: .NET 8.0
- **API Framework**: [FastEndpoints](https://fast-endpoints.com/)
- **ORM**: Entity Framework Core
- **Database**: SQLite
- **Authentication**: JWT Bearer
- **Testing**: xUnit, EF Core InMemory, WebApplicationFactory

## Running the API

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Start the server

From the repository root:

```bash
cd backend
export Jwt__SigningKey='replace-with-a-long-random-local-secret'
dotnet run --project src/MyWebAppFastEndpoints/MyWebAppFastEndpoints.csproj
```

`Jwt__SigningKey` is required at startup and should be provided by your environment (shell, IDE run configuration, or CI secret store).

The API runs on `http://localhost:5116` (or the port in `Properties/launchSettings.json`).

## Database

On first run, the application creates the SQLite database at `db/app.db` and seeds a default admin user.

**Seeded Credentials:**
- **Login:** `admin`
- **Password:** `Admin123!`

## Project Structure

- `src/MyWebAppFastEndpoints/`
  - `Features/Users/` - User endpoints, contracts, domain logic, and persistence.
  - `Features/Posts/` - Post endpoints, contracts, domain logic, and persistence.
  - `Infrastructure/Persistence/` - EF Core context and entities.
  - `Startup/` - Service registration and middleware setup.
- `tests/` - Unit and integration tests.

## Testing

For detailed test setup and coverage reports, see [TESTING.md](./TESTING.md).

```bash
cd backend
dotnet test MyWebAppFastEndpoints.sln
```

## Frontend Integration

The frontend (`../frontend/`) communicates with this API via Next.js route handlers.
See `frontend/README.md` for details.

# Backend - MyWebAppFastEndpoints

This is the backend for the MyWebApp project, built with .NET 8 and utilizing [FastEndpoints](https://fast-endpoints.com/) for a clean and efficient API design.

## Features

- **User Management**: Registration, Login, and Profile management.
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

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Running the Application

1. Navigate to the project directory:
   ```bash
   cd backend/src/MyWebAppFastEndpoints
   ```
2. Run the application:
   ```bash
   dotnet run
   ```
   The API will be available at `http://localhost:5000` (or the port specified in `Properties/launchSettings.json`).

### Database Initialization

The application automatically creates the SQLite database (`app.db`) and seeds an initial admin user on the first run.

**Default Admin Credentials:**
- **Username:** `admin`
- **Password:** `Admin123!`

## Project Structure

- `src/MyWebAppFastEndpoints/` - Main API project.
  - `Data/` - Database context and repository implementations (EF Core).
  - `Users/` - User-related logic, endpoints, and storage interfaces.
  - `Posts/` - Post-related logic, endpoints, and storage interfaces.
- `tests/` - Unit and integration tests.

## Testing

For detailed information on running tests and generating coverage reports, see [TESTING.md](./TESTING.md).

```bash
# Run all tests
dotnet test backend/MyWebAppFastEndpoints.sln
```

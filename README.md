# MyWebAppFastEndpoints Demo

A full-stack demo project that showcases how to build a clean, feature-oriented API with **FastEndpoints** and pair it with a **Next.js** frontend.

The main idea is to demonstrate practical API patterns end-to-end:
- JWT login and role-based authorization
- User management (admin and regular users)
- Post creation, visibility moderation, and reactions
- Simple SQLite persistence for local development

## Stack

- **Backend:** .NET 8, FastEndpoints, EF Core, SQLite, JWT Bearer
- **Frontend:** Next.js 14 (App Router), React 18, TypeScript, Vitest

## Project Layout

```text
.
|- backend/              # See backend/README.md
|  |- src/MyWebAppFastEndpoints/
|  |- tests/
|  |- db/app.db
|- frontend/             # See frontend/README.md
|  |- app/
|  |- lib/
```

## Quick Start

**Prerequisites:** [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), [pnpm](https://pnpm.io/)

1. **Start the backend:**
   ```bash
   cd backend
   dotnet run --project src/MyWebAppFastEndpoints/MyWebAppFastEndpoints.csproj
   ```
   Runs on `http://localhost:5116` by default.

2. **Start the frontend:**
   ```bash
   cd frontend
   cp .env.example .env.local
   pnpm install
   pnpm run dev
   ```
   Runs on `http://localhost:3000` by default.

## Demo Flow (What to Show)

1. Login with seeded admin user: `admin` / `Admin123!`
2. Create a regular user from admin flows.
3. Login as that user and create a post.
4. View public posts, then like/dislike a post.
5. Return as admin and hide/unhide a post.
6. Verify role-protected behavior (`/api/admin/*`, `/api/me/*`, public endpoints).

## Running Tests

```bash
cd backend && dotnet test MyWebAppFastEndpoints.sln
cd frontend && pnpm run test
```

## More Details

- **Backend:** See `backend/README.md` for architecture, features, and API endpoints.
- **Backend Testing:** See `backend/TESTING.md` for coverage and advanced test setup.
- **Frontend:** See `frontend/README.md` for routes and API integration.
- **Database:** Tracked at `backend/db/app.db`; SQLite sidecars (`*.db-shm`, `*.db-wal`) are ignored.

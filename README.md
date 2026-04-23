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
|- backend/
|  |- src/MyWebAppFastEndpoints/   # API project
|  |- tests/                       # xUnit tests
|  |- db/app.db                    # Local demo database (tracked)
|- frontend/
|  |- app/                         # Next.js routes/pages
|  |- lib/                         # API/auth helpers
```

## Quick Start

### 1) Start the backend

```bash
cd backend
dotnet run --project src/MyWebAppFastEndpoints/MyWebAppFastEndpoints.csproj
```

Backend runs on `http://localhost:5116` by default (or your launch profile port).

### 2) Start the frontend

```bash
cd frontend
cp .env.example .env.local
pnpm install
pnpm run dev
```

Open the frontend URL shown by Next.js (commonly `http://localhost:3000`).

## Demo Flow (What to Show)

1. Login with seeded admin user:
   - `admin` / `Admin123!`
2. Create a regular user from admin flows.
3. Login as that user and create a post.
4. View public posts, then like/dislike a post.
5. Return as admin and hide/unhide a post.
6. Verify role-protected behavior (`/api/admin/*`, `/api/me/*`, public endpoints).

## Useful Commands

### Backend tests

```bash
cd backend
dotnet test MyWebAppFastEndpoints.sln
```

### Frontend tests

```bash
cd frontend
pnpm run test
```

## Notes

- Backend database is under `backend/db/app.db`.
- Git is configured to track only `backend/db/app.db` (not SQLite sidecar files).
- More details:
  - `backend/README.md`
  - `backend/TESTING.md`
  - `frontend/README.md`


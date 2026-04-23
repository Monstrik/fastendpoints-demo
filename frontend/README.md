# Frontend (Next.js App Router)

This is the web UI for the FastEndpoints demo app.

It provides auth, profile/status, posts, and admin pages. All backend communication happens via Next.js route handlers under `app/api/*` which proxy to the FastEndpoints API.

## Main Routes

- `/` redirects to `/login` or `/dashboard` based on auth state.
- `/login` login form.
- `/forgot-password` forgot-password form.
- `/dashboard` authenticated user dashboard.
- `/posts` public and admin posts view.
- `/user` authenticated user profile/status page.
- `/users` public user status list.
- `/admin/users` admin-only user management page.

## API Integration

Frontend calls internal routes like `/api/auth/login` and `/api/admin/users`.
Those handlers proxy to the backend FastEndpoints API (e.g., `/api/auth/login`, `/api/users`, `/api/public/posts`, `/api/me`, `/api/me/status`).

`BACKEND_URL` environment variable controls the backend base URL (defaults to `http://localhost:5116` in `lib/api.ts`).

## Setup & Running

See the root `README.md` for full quick start instructions.

Quick reference:

```bash
cd frontend
cp .env.example .env.local
pnpm install
pnpm run dev
```

**Prerequisites:** Backend must be running (see `backend/README.md`).

## Useful Commands

```bash
pnpm run typecheck      # TypeScript type checking
pnpm run test           # Run tests once
pnpm run test:watch     # Watch mode
pnpm run test:coverage  # Coverage report
```

## Implementation Notes

- Auth token is stored in an HTTP-only cookie (`auth_token`).
- Server-side auth/role checks are enforced in app routes with redirects.
- `app/api/*` route handlers centralize backend communication and token forwarding.

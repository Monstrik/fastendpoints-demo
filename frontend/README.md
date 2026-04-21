# Frontend (Next.js App Router)

Minimal frontend for auth + dashboard + admin user management, backed by existing API.

## Pages

- `/` redirects to `/login` or `/dashboard` based on server-side auth check.
- `/login` login form.
- `/forgot-password` forgot-password form.
- `/dashboard` protected page for authenticated users.
- `/admin/users` protected page for admin users only.

## Backend API used

- `POST /auth/login`
- `POST /auth/forgot-password`
- `GET /auth/me`
- `GET /admin/users`
- `POST /admin/users`
- `DELETE /admin/users/:id`

## Setup

1. Copy `.env.example` to `.env.local`.
2. Set `BACKEND_URL` to your API base URL.
3. Install dependencies and start dev server.

```bash
cd frontend
npm install
npm run dev
```

## Type Check

```bash
cd frontend
npm run typecheck
```

## Notes

- Auth token is stored in an HTTP-only cookie (`auth_token`).
- Server components enforce redirects for auth and role checks using `redirect()`.
- Forms and user table are client components.


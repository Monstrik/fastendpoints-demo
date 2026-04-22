import { requireAuth } from "@/lib/auth";

export default async function DashboardPage() {
  const user = await requireAuth();

  return (
    <section>
      <h1>Dashboard</h1>
      <p>ID: {user.id}</p>
      <p>Login: {user.login}</p>
      <p>Name: {user.fullName}</p>
      <p>Age: {user.age}</p>
      <p>Status: {user.status}</p>
      <p>Role: {user.role}</p>
      <p>
        <a href="/user">My Profile</a>
      </p>

      {user.role.toLowerCase() === "admin" && (
        <p>
          <a href="/admin/users">Manage Users</a>
        </p>
      )}

      <form action="/api/auth/logout" method="post">
        <button type="submit">Logout</button>
      </form>
    </section>
  );
}

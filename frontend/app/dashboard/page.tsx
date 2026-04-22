import { requireAuth } from "@/lib/auth";

export default async function DashboardPage() {
  const user = await requireAuth();

  return (
    <section>
      <h1>Dashboard</h1>
      <p>ID: {user.id}</p>
      <p>Login: {user.login}</p>
      <p>Name: {user.fullName}</p>
      <p>Status: {user.status}</p>
      <p>Role: {user.role}</p>
    </section>
  );
}

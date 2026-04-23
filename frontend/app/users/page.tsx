import { backendFetch } from "@/lib/api";
import type { PublicUserStatus } from "@/lib/types";

async function loadUsers(): Promise<{ users: PublicUserStatus[]; error: string | null }> {
  const response = await backendFetch("/api/public/users", { method: "GET" });

  if (!response.ok) {
    return {
      users: [],
      error: `Could not load user statuses (HTTP ${response.status}).`
    };
  }

  return {
    users: (await response.json()) as PublicUserStatus[],
    error: null
  };
}

export default async function UsersStatusPage() {
  const { users, error } = await loadUsers();

  return (
    <section>
      <h1>Users</h1>
      {error ? <p style={{ color: "red" }}>{error}</p> : null}
      <table>
        <thead>
          <tr>
            <th>Login</th>
            <th>Name</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => (
            <tr key={user.login}>
              <td>{user.login}</td>
              <td>{user.fullName}</td>
              <td>{user.status}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  );
}


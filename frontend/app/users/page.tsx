import { backendFetch } from "@/lib/api";
import type { PublicUserStatus } from "@/lib/types";
import { PageHeader } from "@/app/page-header";

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
    <section className="page-shell">
      <PageHeader
        title="Users"
        subtitle="See who is online, what they are focused on, and how the team is feeling today."
      />
      {error ? <p className="page-message page-message-error">{error}</p> : null}
      {!error && users.length === 0 ? (
        <div className="empty-state" role="status">
          <h2 className="empty-state-title">No user statuses yet</h2>
          <p className="empty-state-body">Invite teammates to sign in and update their status.</p>
        </div>
      ) : null}
      {users.length > 0 ? (
        <div className="table-shell">
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
        </div>
      ) : null}
    </section>
  );
}


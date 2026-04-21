import { requireAdmin } from "@/lib/auth";
import { AdminUsersClient } from "@/app/admin/users/users-client";

export default async function AdminUsersPage() {
  await requireAdmin();

  return (
    <section>
      <h1>Admin: Users</h1>
      <AdminUsersClient />
    </section>
  );
}

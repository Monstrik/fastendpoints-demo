import { requireAdmin } from "@/lib/auth";
import { AdminUsersClient } from "@/app/admin/users/users-client";
import { PageHeader } from "@/app/page-header";

export default async function AdminUsersPage() {
  await requireAdmin();

  return (
    <section className="page-shell">
      <PageHeader
        title="User Administration"
        subtitle="Create accounts, manage roles, and keep the workspace roster tidy from one screen."
      />
      <AdminUsersClient />
    </section>
  );
}

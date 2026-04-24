import { requireAuth } from "@/lib/auth";
import { UpdateStatusForm } from "@/app/user/update-status-form";
import { PageHeader } from "@/app/page-header";

export default async function UserPage() {
  const user = await requireAuth();

  return (
    <section className="page-shell">
      <PageHeader
        title="My Profile"
        subtitle="Manage your personal identity and keep your current status fresh for the rest of the team."
      />
      <div className="page-meta-grid">
        <div className="page-meta-item"><span className="page-meta-label">Login</span><span className="page-meta-value">{user.login}</span></div>
        <div className="page-meta-item"><span className="page-meta-label">Name</span><span className="page-meta-value">{user.fullName}</span></div>
        <div className="page-meta-item"><span className="page-meta-label">Role</span><span className="page-meta-value">{user.role}</span></div>
      </div>
      <UpdateStatusForm currentStatus={user.status} />
    </section>
  );
}

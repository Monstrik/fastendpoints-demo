import { requireAuth } from "@/lib/auth";
import { UpdateStatusForm } from "@/app/user/update-status-form";

export default async function UserPage() {
  const user = await requireAuth();

  return (
    <section>
      <h1>My Profile</h1>
      {/*<p>ID: {user.id}</p>*/}
      <p>Login: {user.login}</p>
      <p>Name: {user.fullName}</p>
      <p>Role: {user.role}</p>
      <UpdateStatusForm currentStatus={user.status} />
    </section>
  );
}

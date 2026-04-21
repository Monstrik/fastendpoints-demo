import { requireAuth } from "@/lib/auth";
import { UpdateAgeForm } from "@/app/user/update-age-form";

export default async function UserPage() {
  const user = await requireAuth();

  return (
    <section>
      <h1>My Profile</h1>
      <p>Login: {user.login}</p>
      <p>Current age: {user.age}</p>
      <UpdateAgeForm currentAge={user.age} />
      <p>
        <a href="/dashboard">Back to dashboard</a>
      </p>
    </section>
  );
}


import { redirect } from "next/navigation";
import { getCurrentUser } from "@/lib/auth";
import { LoginForm } from "@/app/login/login-form";

export default async function LoginPage() {
  const user = await getCurrentUser();

  if (user) {
    redirect("/dashboard");
  }

  return (
    <section>
      <h1>Login</h1>
      <LoginForm />
      <p>
        <a href="/forgot-password">Forgot password?</a>
      </p>
      <p>
        <a href="/users">View user statuses</a>
      </p>
    </section>
  );
}

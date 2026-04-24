import { redirect } from "next/navigation";
import { getCurrentUser } from "@/lib/auth";
import { LoginForm } from "@/app/login/login-form";
import { PageHeader } from "@/app/page-header";

export default async function LoginPage() {
  const user = await getCurrentUser();

  if (user) {
    redirect("/dashboard");
  }

  return (
    <section className="page-shell">
      <PageHeader
        title="Login"
        subtitle="Sign in to post updates, react to team activity, and manage your workspace access."
      />
      <LoginForm />
      <p>
        <a href="/forgot-password">Forgot password?</a>
      </p>
    </section>
  );
}

import { ForgotPasswordForm } from "@/app/forgot-password/forgot-password-form";
import { PageHeader } from "@/app/page-header";

export default function ForgotPasswordPage() {
  return (
    <section className="page-shell">
      <PageHeader
        title="Forgot Password"
        subtitle="Request a reset link and regain access to your account without contacting an administrator."
      />
      <ForgotPasswordForm />
      <p>
        <a href="/login">Back to login</a>
      </p>
    </section>
  );
}

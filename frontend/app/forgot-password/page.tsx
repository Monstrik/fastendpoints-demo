import { ForgotPasswordForm } from "@/app/forgot-password/forgot-password-form";

export default function ForgotPasswordPage() {
  return (
    <section>
      <h1>Forgot Password</h1>
      <ForgotPasswordForm />
      <p>
        <a href="/login">Back to login</a>
      </p>
    </section>
  );
}

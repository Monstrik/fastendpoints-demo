"use client";

import { FormEvent, useState } from "react";

export function ForgotPasswordForm() {
  const [login, setLogin] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage(null);
    setError(null);
    setIsSubmitting(true);

    try {
      const response = await fetch("/api/auth/forgot-password", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ login }),
        credentials: "include"
      });

      if (!response.ok) {
        const body = (await response.json().catch(() => null)) as { message?: string } | null;
        setError(body?.message ?? "Could not submit forgot-password request.");
        return;
      }

      setMessage("If your login exists, password reset instructions were sent.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={onSubmit}>
      <label htmlFor="login">Login</label>
      <input
        id="login"
        type="text"
        value={login}
        onChange={(event) => setLogin(event.target.value)}
        required
      />

      {message ? <p>{message}</p> : null}
      {error ? <p style={{ color: "red" }}>{error}</p> : null}

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? "Submitting..." : "Send reset link"}
      </button>
    </form>
  );
}

"use client";

import { FormEvent, useState } from "react";

type Props = {
  currentAge: number;
};

export function UpdateAgeForm({ currentAge }: Props) {
  const [age, setAge] = useState(currentAge);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage(null);
    setError(null);
    setIsSubmitting(true);

    try {
      const response = await fetch("/api/me/age", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify({ age })
      });

      if (!response.ok) {
        const body = (await response.json().catch(() => null)) as
          | { message?: string; errors?: Record<string, string[]> }
          | null;
        const errorMessage =
          body?.errors ? Object.values(body.errors).flat()[0] : body?.message;
        setError(errorMessage ?? "Could not update age.");
        return;
      }

      const updated = (await response.json()) as { age: number };
      setAge(updated.age);
      setMessage("Age updated successfully.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={onSubmit}>
      <label htmlFor="age">Age</label>
      <input
        id="age"
        type="number"
        min={1}
        max={130}
        value={age}
        onChange={(event) => setAge(parseInt(event.target.value, 10))}
        required
      />

      {message ? <p>{message}</p> : null}
      {error ? <p style={{ color: "red" }}>{error}</p> : null}

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? "Saving..." : "Save age"}
      </button>
    </form>
  );
}


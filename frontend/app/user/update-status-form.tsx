"use client";

import { useState } from "react";

const statusOptions = [
  "🟢 Available",
  "🔴 Busy",
  "🔕 Do not disturb",
  "🎯 Focused",
  "⛔ Blocked",
  "🍽️ Eating",
  "🔨 In progress",
  "👀 Reviewing",
  "🧪 Testing",
  "🎨 Designing",
  "🔍 Researching",
  "📝 Documenting",
  "⏳ Waiting for input",
  "🏠 Working from home",
  "🏢 In office",
  "✈️ Traveling",
  "🚶 Stepped away",
  "🐢 Delayed response",
  "🌴 Out of office (OOO)",
  "🏖️ On vacation",
  "🤒 Sick leave",
  "🤝 Waiting on another team",
  "🧑‍💼 Waiting on customer",
  "⚫ Offline",
  "🆕 Onboarding",
  "📟 On-call"
] as const;

type Props = {
  currentStatus: string;
};

export function UpdateStatusForm({ currentStatus }: Props) {
  const [status, setStatus] = useState(currentStatus);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function updateStatus(nextStatus: string) {
    if (nextStatus === status || isSubmitting) {
      return;
    }

    if (!statusOptions.includes(nextStatus as (typeof statusOptions)[number])) {
      setError("Invalid status selected.");
      return;
    }

    setMessage(null);
    setError(null);
    setIsSubmitting(true);
    const previousStatus = status;
    setStatus(nextStatus);

    try {
      const response = await fetch("/api/me/status", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify({ status: nextStatus })
      });

      if (!response.ok) {
        const body = (await response.json().catch(() => null)) as
          | { message?: string; errors?: Record<string, string[]> }
          | null;
        const errorMessage =
          body?.errors ? Object.values(body.errors).flat()[0] : body?.message;
        setError(errorMessage ?? "Could not update status.");
        setStatus(previousStatus);
        return;
      }

      const updated = (await response.json()) as { status: string };
      setStatus(updated.status);
      setMessage("Status updated successfully.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form>
      <label htmlFor="status">Status</label>
      <select
        id="status"
        value={status}
        onChange={(event) => {
          void updateStatus(event.target.value);
        }}
        disabled={isSubmitting}
        required
      >
        {statusOptions.map((option) => (
          <option key={option} value={option}>
            {option}
          </option>
        ))}
      </select>

      {message ? <p>{message}</p> : null}
      {error ? <p style={{ color: "red" }}>{error}</p> : null}
    </form>
  );
}


"use client";

import { FormEvent, useState } from "react";

const statusOptions = [
  "🟢 Available",
  "💻 Online",
  "🛠️ Working",
  "🎯 Focused",
  "🧠 Heads-down",
  "✅ On task",
  "⚡ Active",
  "🆓 Free",
  "🔴 Busy",
  "📅 In a meeting",
  "🎤 Presenting",
  "📞 On a call",
  "🔕 Do not disturb",
  "🧑‍💻 Deep work",
  "⛔ Blocked",
  "🔄 Context switching",
  "🍽️ Eating",
  "🔨 In progress",
  "👀 Reviewing",
  "💻 Coding",
  "🧪 Testing",
  "🐞 Debugging",
  "🎨 Designing",
  "🔍 Researching",
  "📝 Documenting",
  "⏳ Waiting for input",
  "✅ Waiting for approval",
  "🏠 Working from home",
  "🏢 In office",
  "🌍 Remote",
  "✈️ Traveling",
  "🔜 Back soon",
  "🚶 Stepped away",
  "🕒 Away briefly",
  "🐢 Delayed response",
  "⏱️ Limited availability",
  "🌴 Out of office (OOO)",
  "🏖️ On vacation",
  "🤒 Sick leave",
  "🧑‍⚕️ Personal leave",
  "🎉 Holiday",
  "🛌 Day off",
  "🚧 Blocked by dependency",
  "🤝 Waiting on another team",
  "🧑‍💼 Waiting on customer",
  "📋 Awaiting requirements",
  "⚫ Offline",
  "❓ Unknown",
  "🆕 Onboarding",
  "📟 On-call",
  "🌙 After hours"
] as const;

type Props = {
  currentStatus: string;
};

export function UpdateStatusForm({ currentStatus }: Props) {
  const [status, setStatus] = useState(currentStatus);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage(null);
    setError(null);
    setIsSubmitting(true);

    try {
      const response = await fetch("/api/me/status", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify({ status })
      });

      if (!response.ok) {
        const body = (await response.json().catch(() => null)) as
          | { message?: string; errors?: Record<string, string[]> }
          | null;
        const errorMessage =
          body?.errors ? Object.values(body.errors).flat()[0] : body?.message;
        setError(errorMessage ?? "Could not update status.");
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
    <form onSubmit={onSubmit}>
      <label htmlFor="status">Status</label>
      <select
        id="status"
        value={status}
        onChange={(event) => setStatus(event.target.value)}
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

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? "Saving..." : "Save status"}
      </button>
    </form>
  );
}

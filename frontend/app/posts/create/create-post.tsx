"use client";

import { FormEvent, useState } from "react";

type Props = {
  canPost: boolean;
};

const MAX_POST_LENGTH = 280;

export function CreatePost({ canPost }: Props) {
  const [content, setContent] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  async function onCreatePost(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!canPost || isSubmitting) {
      return;
    }

    const trimmed = content.trim();

    if (!trimmed) {
      setError("Post content is required.");
      return;
    }

    if (trimmed.length > MAX_POST_LENGTH) {
      setError(`Post content must be ${MAX_POST_LENGTH} characters or less.`);
      return;
    }

    setIsSubmitting(true);
    setError(null);
    setMessage(null);

    try {
      const response = await fetch("/api/posts", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify({ content: trimmed })
      });

      if (!response.ok) {
        const body = (await response.json().catch(() => null)) as
          | { message?: string; errors?: Record<string, string[]> }
          | null;
        const errorMessage =
          body?.errors ? Object.values(body.errors).flat()[0] : body?.message;
        setError(errorMessage ?? "Could not create post.");
        return;
      }

      setContent("");
      setMessage("Post published.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div>
      {canPost ? (
        <form onSubmit={onCreatePost}>
          <label htmlFor="post-content">Write a short post</label>
          <textarea
            id="post-content"
            value={content}
            onChange={(event) => setContent(event.target.value)}
            maxLength={MAX_POST_LENGTH}
            rows={3}
            placeholder="Share a quick update"
            required
          />
          <p>{content.length}/{MAX_POST_LENGTH}</p>
          <button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Posting..." : "Post"}
          </button>
        </form>
      ) : (
        <p>Log in to publish posts.</p>
      )}

      {message ? <p>{message}</p> : null}
      {error ? <p style={{ color: "red" }}>{error}</p> : null}
    </div>
  );
}


"use client";

import { FormEvent, useState } from "react";
import type { PublicPost } from "@/lib/types";

type Props = {
  initialPosts: PublicPost[];
  canPost: boolean;
  isAdmin: boolean;
};

const MAX_POST_LENGTH = 280;

export function PostsClient({ initialPosts, canPost, isAdmin }: Props) {
  const [posts, setPosts] = useState<PublicPost[]>(initialPosts);
  const [content, setContent] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  async function reloadPosts() {
    const response = await fetch("/api/posts", {
      method: "GET",
      credentials: "include",
      cache: "no-store"
    });

    if (!response.ok) {
      setError("Could not refresh posts.");
      return;
    }

    const data = (await response.json()) as PublicPost[];
    setPosts(data);
  }

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
      await reloadPosts();
    } finally {
      setIsSubmitting(false);
    }
  }

  async function onHidePost(id: string) {
    if (!isAdmin || isSubmitting) {
      return;
    }

    setIsSubmitting(true);
    setError(null);
    setMessage(null);

    try {
      const response = await fetch(`/api/posts/${id}/hide`, {
        method: "PUT",
        credentials: "include"
      });

      if (!response.ok) {
        setError("Could not hide post.");
        return;
      }

      setMessage("Post hidden.");
      await reloadPosts();
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

      <h2>Posts</h2>
      {posts.length === 0 ? (
        <p>No posts yet.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Author</th>
              <th>Post</th>
              <th>Created</th>
              {isAdmin ? <th>Actions</th> : null}
            </tr>
          </thead>
          <tbody>
            {posts.map((post) => (
              <tr key={post.id}>
                <td>{post.authorLogin}</td>
                <td>{post.content}</td>
                <td>{new Date(post.createdAtUtc).toLocaleString()}</td>
                {isAdmin ? (
                  <td>
                    <button type="button" onClick={() => onHidePost(post.id)} disabled={isSubmitting}>
                      Hide
                    </button>
                  </td>
                ) : null}
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}


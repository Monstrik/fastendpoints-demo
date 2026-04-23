"use client";

import type { MyPost, PublicPost } from "@/lib/types";

type Props = {
  post: PublicPost | MyPost;
  canReact?: boolean;
  isReacting?: boolean;
  onReact?: (post: PublicPost | MyPost, reaction: "Like" | "Dislike") => void;
  canModerate?: boolean;
  isSubmitting?: boolean;
  onToggleVisibility?: (post: MyPost) => void;
};

function timeAgo(dateStr: string): string {
  const date = parsePostDate(dateStr);
  if (Number.isNaN(date.getTime())) return "Unknown time";

  const diff = Math.max(0, Date.now() - date.getTime());
  const mins = Math.floor(diff / 60_000);
  if (mins < 1) return "just now";
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return date.toLocaleDateString();
}

function parsePostDate(dateStr: string): Date {
  const normalized = /(?:Z|[+-]\d{2}:\d{2})$/i.test(dateStr) ? dateStr : `${dateStr}Z`;
  return new Date(normalized);
}

function formatPostDate(dateStr: string): string {
  const date = parsePostDate(dateStr);

  if (Number.isNaN(date.getTime())) {
    return "Unknown date";
  }

  return date.toLocaleDateString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric"
  });
}

export function PostCard({
  post,
  canReact = false,
  isReacting = false,
  onReact,
  canModerate = false,
  isSubmitting = false,
  onToggleVisibility
}: Props) {
  const visibility = "isHidden" in post ? (post.isHidden ? "Hidden" : "Public") : null;
  const canToggle = canModerate && "isHidden" in post && !!onToggleVisibility;

  return (
    <article className="post-card">
      <div className="post-card-header">
        <span className="post-card-avatar">
          {post.authorLogin.charAt(0).toUpperCase()}
        </span>
        <div className="post-card-meta">
          <span className="post-card-author">@{post.authorLogin}</span>
          <span className="post-card-status">{post.authorStatus}</span>
          <div className="post-card-timestamps" title={parsePostDate(post.createdAtUtc).toLocaleString()}>
            <span className="post-card-time">{timeAgo(post.createdAtUtc)}</span>
            <span className="post-card-date">{formatPostDate(post.createdAtUtc)}</span>
          </div>
        </div>
        {visibility ? (
          canToggle ? (
            <button
              type="button"
              className={`post-card-visibility ${visibility === "Hidden" ? "is-hidden" : "is-public"}`}
              onClick={() => onToggleVisibility(post)}
              disabled={isSubmitting}
              title={post.isHidden ? "Click to unhide post" : "Click to hide post"}
            >
              {visibility}
            </button>
          ) : (
            <span className={`post-card-visibility ${visibility === "Hidden" ? "is-hidden" : "is-public"}`}>
              {visibility}
            </span>
          )
        ) : null}
      </div>
      <p className="post-card-content">{post.content}</p>
      <div className="post-card-reactions">
        <button
          type="button"
          className={`reaction-button ${post.viewerReaction === "Like" ? "is-active" : ""}`}
          onClick={() => onReact?.(post, "Like")}
          disabled={!canReact || isReacting}
          aria-label={`Like post. ${post.likesCount} likes.`}
          aria-pressed={post.viewerReaction === "Like"}
          title="Like"
        >
          <span className="reaction-icon" aria-hidden="true">👍</span>
          <span className="reaction-count">{post.likesCount}</span>
        </button>
        <button
          type="button"
          className={`reaction-button ${post.viewerReaction === "Dislike" ? "is-active" : ""}`}
          onClick={() => onReact?.(post, "Dislike")}
          disabled={!canReact || isReacting}
          aria-label={`Dislike post. ${post.dislikesCount} dislikes.`}
          aria-pressed={post.viewerReaction === "Dislike"}
          title="Dislike"
        >
          <span className="reaction-icon" aria-hidden="true">👎</span>
          <span className="reaction-count">{post.dislikesCount}</span>
        </button>
      </div>
    </article>
  );
}


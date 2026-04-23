import type { MyPost, PublicPost } from "@/lib/types";

type Props = {
  post: PublicPost | MyPost;
};

function timeAgo(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const mins = Math.floor(diff / 60_000);
  if (mins < 1) return "just now";
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(dateStr).toLocaleDateString();
}

export function PostCard({ post }: Props) {
  const visibility = "isHidden" in post ? (post.isHidden ? "Hidden" : "Public") : null;

  return (
    <article className="post-card">
      <div className="post-card-header">
        <span className="post-card-avatar">
          {post.authorLogin.charAt(0).toUpperCase()}
        </span>
        <div className="post-card-meta">
          <span className="post-card-author">@{post.authorLogin}</span>
          <span className="post-card-time" title={new Date(post.createdAtUtc).toLocaleString()}>
            {timeAgo(post.createdAtUtc)}
          </span>
        </div>
        {visibility ? (
          <span className={`post-card-visibility ${visibility === "Hidden" ? "is-hidden" : "is-public"}`}>
            {visibility}
          </span>
        ) : null}
      </div>
      <p className="post-card-content">{post.content}</p>
    </article>
  );
}


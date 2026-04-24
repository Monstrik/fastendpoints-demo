import { requireAuth } from "@/lib/auth";
import { backendFetch } from "@/lib/api";
import { getAuthToken } from "@/lib/auth";
import type { MyPost } from "@/lib/types";
import Link from "next/link";
import { PostCard } from "@/app/posts/post-card";
import { PageHeader } from "@/app/page-header";

async function loadMyPosts(): Promise<{ posts: MyPost[]; error: string | null }> {
  const token = getAuthToken();

  if (!token) {
    return {
      posts: [],
      error: "Could not load your posts."
    };
  }

  const response = await backendFetch("/api/me/posts", {
    method: "GET",
    token
  });

  if (!response.ok) {
    return {
      posts: [],
      error: `Could not load your posts (HTTP ${response.status}).`
    };
  }

  return {
    posts: (await response.json()) as MyPost[],
    error: null
  };
}

export default async function DashboardPage() {
  const [user, { posts, error }] = await Promise.all([requireAuth(), loadMyPosts()]);

  return (
    <section className="page-shell">
      <PageHeader
        title="Dashboard"
        subtitle="Your personal overview with account details, current status, and the posts you've shared."
        actions={<Link href="/posts/create" className="page-link-inline">Create New Post</Link>}
      />
      <div className="page-meta-grid">
        <div className="page-meta-item"><span className="page-meta-label">ID</span><span className="page-meta-value">{user.id}</span></div>
        <div className="page-meta-item"><span className="page-meta-label">Login</span><span className="page-meta-value">{user.login}</span></div>
        <div className="page-meta-item"><span className="page-meta-label">Name</span><span className="page-meta-value">{user.fullName}</span></div>
        <div className="page-meta-item"><span className="page-meta-label">Status</span><span className="page-meta-value">{user.status}</span></div>
        <div className="page-meta-item"><span className="page-meta-label">Role</span><span className="page-meta-value">{user.role}</span></div>
      </div>
      <h2>My Posts</h2>
      {error ? <p className="page-message page-message-error">{error}</p> : null}
      {posts.length === 0 ? (
        <div className="empty-state" role="status">
          <h3 className="empty-state-title">You have not posted yet</h3>
          <p className="empty-state-body">Create your first post to share progress, blockers, or updates with the team.</p>
        </div>
      ) : (
        <div className="post-feed">
          {posts.map((post) => (
            <PostCard key={post.id} post={post} />
          ))}
        </div>
      )}
    </section>
  );
}

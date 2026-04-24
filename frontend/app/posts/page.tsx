import { backendFetch } from "@/lib/api";
import { getAuthToken, getCurrentUser } from "@/lib/auth";
import type { MyPost, PublicPost } from "@/lib/types";
import { PostsFeedClient } from "@/app/posts/posts-feed-client";
import { PageHeader } from "@/app/page-header";

async function loadPosts(): Promise<{ posts: PublicPost[]; error: string | null }> {
  const token = getAuthToken();

  const response = await backendFetch("/api/public/posts", {
    method: "GET",
    token
  });

  if (!response.ok) {
    return {
      posts: [],
      error: `Could not load posts (HTTP ${response.status}).`
    };
  }

  return {
    posts: (await response.json()) as PublicPost[],
    error: null
  };
}

async function loadAdminPosts(token: string): Promise<{ posts: MyPost[]; error: string | null }> {
  const response = await backendFetch("/api/admin/posts", {
    method: "GET",
    token
  });

  if (!response.ok) {
    return {
      posts: [],
      error: `Could not load all posts (HTTP ${response.status}).`
    };
  }

  return {
    posts: (await response.json()) as MyPost[],
    error: null
  };
}

export default async function PostsPage() {
  const [user, publicResult] = await Promise.all([getCurrentUser(), loadPosts()]);
  const isAdmin = user?.role.toLowerCase() === "admin";

  let posts: Array<PublicPost | MyPost> = publicResult.posts;
  let error = publicResult.error;

  if (isAdmin) {
    const token = getAuthToken();

    if (token) {
      const adminResult = await loadAdminPosts(token);
      posts = adminResult.posts;
      error = adminResult.error;
    }
  }

  return (
    <section className="page-shell">
      <PageHeader
        title={isAdmin ? "All Posts" : "Posts"}
        subtitle={isAdmin ? "Review visibility, moderate content, and watch community activity in one place." : "Catch up on the latest updates, reactions, and conversations from the team."}
      />
      {error ? <p className="page-message page-message-error">{error}</p> : null}
      <PostsFeedClient initialPosts={posts} canModerate={isAdmin} canReact={!!user} />
    </section>
  );
}


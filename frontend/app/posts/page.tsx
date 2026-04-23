import { backendFetch } from "@/lib/api";
import { getAuthToken, getCurrentUser } from "@/lib/auth";
import type { MyPost, PublicPost } from "@/lib/types";
import { PostsFeedClient } from "@/app/posts/posts-feed-client";

async function loadPosts(): Promise<{ posts: PublicPost[]; error: string | null }> {
  const response = await backendFetch("/api/public/posts", { method: "GET" });

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
    <section>
      <h1>{isAdmin ? "All Posts" : "Public Posts"}</h1>
      {error ? <p style={{ color: "red" }}>{error}</p> : null}
      <PostsFeedClient initialPosts={posts} canModerate={isAdmin} />
    </section>
  );
}


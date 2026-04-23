import { backendFetch } from "@/lib/api";
import { getCurrentUser } from "@/lib/auth";
import type { PublicPost } from "@/lib/types";
import { PostsClient } from "@/app/posts/posts-client";

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

export default async function PostsPage() {
  const [{ posts, error }, user] = await Promise.all([loadPosts(), getCurrentUser()]);
  const isAdmin = user?.role.toLowerCase() === "admin";

  return (
    <section>
      <h1>Public Posts</h1>
      {error ? <p style={{ color: "red" }}>{error}</p> : null}
      <PostsClient initialPosts={posts} canPost={!!user} isAdmin={isAdmin} />
    </section>
  );
}


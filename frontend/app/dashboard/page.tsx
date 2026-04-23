import { requireAuth } from "@/lib/auth";
import { backendFetch } from "@/lib/api";
import { getAuthToken } from "@/lib/auth";
import type { MyPost } from "@/lib/types";
import Link from "next/link";

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
    <section>
      <h1>Dashboard</h1>
      <p>ID: {user.id}</p>
      <p>Login: {user.login}</p>
      <p>Name: {user.fullName}</p>
      <p>Status: {user.status}</p>
      <p>Role: {user.role}</p>
      <Link href="/posts/create">Create New Post</Link>
      <h2>My Posts</h2>
      {error ? <p style={{ color: "red" }}>{error}</p> : null}
      {posts.length === 0 ? (
        <p>You have not posted yet.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Post</th>
              <th>Created</th>
              <th>Visibility</th>
            </tr>
          </thead>
          <tbody>
            {posts.map((post) => (
              <tr key={post.id}>
                <td>{post.content}</td>
                <td>{new Date(post.createdAtUtc).toLocaleString()}</td>
                <td>{post.isHidden ? "Hidden" : "Public"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}

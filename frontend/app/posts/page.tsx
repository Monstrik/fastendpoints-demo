import { backendFetch } from "@/lib/api";
import type { PublicPost } from "@/lib/types";

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
  const { posts, error } = await loadPosts();

  return (
    <section>
      <h1>Public Posts</h1>
      {error ? <p style={{ color: "red" }}>{error}</p> : null}
      {posts.length === 0 ? (
        <p>No posts yet.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Author</th>
              <th>Post</th>
              <th>Created</th>
            </tr>
          </thead>
          <tbody>
            {posts.map((post) => (
              <tr key={post.id}>
                <td>{post.authorLogin}</td>
                <td>{post.content}</td>
                <td>{new Date(post.createdAtUtc).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}


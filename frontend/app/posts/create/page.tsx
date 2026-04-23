import { getCurrentUser } from "@/lib/auth";
import { CreatePost } from "@/app/posts/create/create-post";

export default async function PostsClientPage() {
  const user = await getCurrentUser();

  return (
    <section>
      <h1>Create Post</h1>
      <CreatePost canPost={!!user} />
    </section>
  );
}

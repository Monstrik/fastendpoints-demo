import { getCurrentUser } from "@/lib/auth";
import { CreatePost } from "@/app/posts/create/create-post";
import { PageHeader } from "@/app/page-header";

export default async function PostsClientPage() {
  const user = await getCurrentUser();

  return (
    <section className="page-shell">
      <PageHeader
        title="Create Post"
        subtitle="Share a concise update, question, or progress note with the team timeline."
      />
      <CreatePost canPost={!!user} />
    </section>
  );
}

"use client";

import { useState } from "react";
import type { MyPost, PublicPost } from "@/lib/types";
import { PostCard } from "@/app/posts/post-card";

type PostItem = PublicPost | MyPost;

type Props = {
  initialPosts: PostItem[];
  canModerate: boolean;
};

function isModeratedPost(post: PostItem): post is MyPost {
  return "isHidden" in post;
}

export function PostsFeedClient({ initialPosts, canModerate }: Props) {
  const [posts, setPosts] = useState<PostItem[]>(initialPosts);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function toggleVisibility(post: MyPost) {
    if (!canModerate || isSubmitting) {
      return;
    }

    setError(null);
    setIsSubmitting(true);

    const endpoint = post.isHidden ? `/api/posts/${post.id}/unhide` : `/api/posts/${post.id}/hide`;

    try {
      const response = await fetch(endpoint, {
        method: "PUT",
        credentials: "include"
      });

      if (!response.ok) {
        setError(`Could not ${post.isHidden ? "unhide" : "hide"} post.`);
        return;
      }

      setPosts((current) =>
        current.map((item) =>
          item.id === post.id && isModeratedPost(item)
            ? { ...item, isHidden: !item.isHidden }
            : item
        )
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  if (posts.length === 0) {
    return <p>No posts yet.</p>;
  }

  return (
    <div className="post-feed">
      {error ? <p style={{ color: "red" }}>{error}</p> : null}
      {posts.map((post) => (
        <div key={post.id} className="post-card-wrap">
          <PostCard post={post} />
          {canModerate && isModeratedPost(post) ? (
            <div className="post-card-actions">
              <button
                type="button"
                onClick={() => toggleVisibility(post)}
                disabled={isSubmitting}
                className="secondary"
              >
                {post.isHidden ? "Unhide" : "Hide"}
              </button>
            </div>
          ) : null}
        </div>
      ))}
    </div>
  );
}


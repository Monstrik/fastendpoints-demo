import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { PostsFeedClient } from "@/app/posts/posts-feed-client";
import type { MyPost, PublicPost } from "@/lib/types";

describe("PostsFeedClient", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it("updates reaction counts after liking a post", async () => {
    const fetchMock = vi.spyOn(global, "fetch").mockResolvedValue({ ok: true } as Response);

    const post: PublicPost = {
      id: "post-1",
      authorLogin: "aya",
      authorStatus: "🟢 Available",
      content: "React to me",
      createdAtUtc: "2026-04-23T10:00:00Z",
      likesCount: 0,
      dislikesCount: 0,
      viewerReaction: null
    };

    render(<PostsFeedClient initialPosts={[post]} canModerate={false} canReact />);

    await userEvent.click(screen.getByRole("button", { name: /like post\. 0 likes\./i }));

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith("/api/posts/post-1/reaction", expect.objectContaining({
        method: "PUT"
      }));
      expect(screen.getByRole("button", { name: /like post\. 1 likes\./i })).toHaveAttribute("aria-pressed", "true");
    });
  });

  it("toggles visibility for moderated posts", async () => {
    const fetchMock = vi.spyOn(global, "fetch").mockResolvedValue({ ok: true } as Response);

    const post: MyPost = {
      id: "post-2",
      authorLogin: "admin",
      authorStatus: "🔴 Busy",
      content: "Moderate me",
      createdAtUtc: "2026-04-23T10:00:00Z",
      isHidden: false,
      likesCount: 0,
      dislikesCount: 0,
      viewerReaction: null
    };

    render(<PostsFeedClient initialPosts={[post]} canModerate canReact={false} />);

    await userEvent.click(screen.getByRole("button", { name: "Public" }));

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith("/api/posts/post-2/hide", expect.objectContaining({
        method: "PUT"
      }));
      expect(screen.getByRole("button", { name: "Hidden" })).toBeInTheDocument();
    });
  });
});


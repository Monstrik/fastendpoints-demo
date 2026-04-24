import { fireEvent, render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { PostCard } from "@/app/posts/post-card";
import type { PublicPost } from "@/lib/types";

describe("PostCard", () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date("2026-04-23T12:00:00Z"));
  });

  it("renders author status and parses UTC timestamps without timezone suffix", () => {
    const post: PublicPost = {
      id: "post-1",
      authorLogin: "aya",
      authorStatus: "🟢 Available",
      content: "Hello from the timeline",
      createdAtUtc: "2026-04-23T10:00:00",
      likesCount: 2,
      dislikesCount: 1,
      viewerReaction: null
    };

    render(<PostCard post={post} />);

    expect(screen.getByText("@aya")).toBeInTheDocument();
    expect(screen.getByText("🟢 Available")).toBeInTheDocument();
    expect(screen.getByText("2h ago")).toBeInTheDocument();
    expect(screen.getByText("Apr 23, 2026")).toBeInTheDocument();
  });

  it("renders visibility badge and supports moderation toggles", async () => {
    const onToggleVisibility = vi.fn();

    render(
      <PostCard
        post={{
          id: "post-2",
          authorLogin: "admin",
          authorStatus: "🔴 Busy",
          content: "Moderated",
          createdAtUtc: "2026-04-23T10:00:00Z",
          isHidden: true,
          likesCount: 0,
          dislikesCount: 0,
          viewerReaction: "Like"
        }}
        canModerate
        onToggleVisibility={onToggleVisibility}
      />
    );

    expect(screen.getByRole("button", { name: "Hidden" })).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Hidden" }));
    expect(onToggleVisibility).toHaveBeenCalled();
  });

  it("shows unknown date text for invalid timestamps", () => {
    render(
      <PostCard
        post={{
          id: "post-3",
          authorLogin: "aya",
          authorStatus: "🟢 Available",
          content: "Bad date",
          createdAtUtc: "not-a-date",
          likesCount: 0,
          dislikesCount: 0,
          viewerReaction: null
        }}
      />
    );

    expect(screen.getByText("Unknown time")).toBeInTheDocument();
    expect(screen.getByText("Unknown date")).toBeInTheDocument();
  });

  it("shows view icon and likes count for non-authenticated users", () => {
    const post: PublicPost = {
      id: "post-4",
      authorLogin: "aya",
      authorStatus: "🟢 Available",
      content: "This is a public post",
      createdAtUtc: "2026-04-23T10:00:00",
      likesCount: 5,
      dislikesCount: 2,
      viewerReaction: null
    };

    render(<PostCard post={post} canReact={false} />);

    // Should show like and dislike icons without buttons
    const likeIcons = screen.getAllByText("👍");
    const dislikeIcons = screen.getAllByText("👎");
    expect(likeIcons.length).toBeGreaterThan(0);
    expect(dislikeIcons.length).toBeGreaterThan(0);
    // Should show likes and dislikes counts
    expect(screen.getByText("5")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    // Should not show as buttons (should be divs)
    expect(screen.queryByRole("button", { name: /Like post/i })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Dislike post/i })).not.toBeInTheDocument();
  });

  it("shows like and dislike buttons for authenticated users", () => {
    const onReact = vi.fn();
    const post: PublicPost = {
      id: "post-5",
      authorLogin: "aya",
      authorStatus: "🟢 Available",
      content: "This is a public post",
      createdAtUtc: "2026-04-23T10:00:00",
      likesCount: 5,
      dislikesCount: 2,
      viewerReaction: null
    };

    render(<PostCard post={post} canReact={true} onReact={onReact} />);

    // Should show like and dislike buttons
    expect(screen.getByText("👍")).toBeInTheDocument();
    expect(screen.getByText("👎")).toBeInTheDocument();
    // Should not show view icon
    expect(screen.queryByText("👁️")).not.toBeInTheDocument();
  });

  it("applies stronger active state classes to the selected reaction", () => {
    render(
      <PostCard
        post={{
          id: "post-6",
          authorLogin: "aya",
          authorStatus: "🟢 Available",
          content: "Liked already",
          createdAtUtc: "2026-04-23T10:00:00Z",
          likesCount: 3,
          dislikesCount: 0,
          viewerReaction: "Like"
        }}
        canReact
        onReact={vi.fn()}
      />
    );

    expect(screen.getByRole("button", { name: /like post\. 3 likes\./i })).toHaveClass("reaction-like", "is-active");
    expect(screen.getByRole("button", { name: /dislike post\. 0 dislikes\./i })).toHaveClass("reaction-dislike");
  });
});


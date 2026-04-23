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
});


import { render, screen } from "@testing-library/react";
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
});


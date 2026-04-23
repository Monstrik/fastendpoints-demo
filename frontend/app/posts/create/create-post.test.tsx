import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { CreatePost } from "@/app/posts/create/create-post";

const { pushMock, refreshMock } = vi.hoisted(() => ({
  pushMock: vi.fn(),
  refreshMock: vi.fn()
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: pushMock,
    refresh: refreshMock
  })
}));

describe("CreatePost", () => {
  beforeEach(() => {
    pushMock.mockReset();
    refreshMock.mockReset();
  });

  it("creates a post and redirects to dashboard", async () => {
    const fetchMock = vi.spyOn(global, "fetch").mockResolvedValue({ ok: true } as Response);

    render(<CreatePost canPost />);

    await userEvent.type(screen.getByLabelText(/write a short post/i), "Hello team");
    await userEvent.click(screen.getByRole("button", { name: /^post$/i }));

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith("/api/posts", expect.objectContaining({ method: "POST" }));
      expect(pushMock).toHaveBeenCalledWith("/dashboard");
      expect(refreshMock).toHaveBeenCalled();
    });
  });

  it("validates empty content before calling fetch", async () => {
    const fetchMock = vi.spyOn(global, "fetch");

    render(<CreatePost canPost />);

    await userEvent.type(screen.getByLabelText(/write a short post/i), "   ");
    await userEvent.click(screen.getByRole("button", { name: /^post$/i }));

    expect(await screen.findByText("Post content is required.")).toBeInTheDocument();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("shows backend error when creation fails", async () => {
    vi.spyOn(global, "fetch").mockResolvedValue({
      ok: false,
      json: vi.fn().mockResolvedValue({ message: "Could not create post." })
    } as unknown as Response);

    render(<CreatePost canPost />);

    await userEvent.type(screen.getByLabelText(/write a short post/i), "Hello team");
    await userEvent.click(screen.getByRole("button", { name: /^post$/i }));

    expect(await screen.findByText("Could not create post.")).toBeInTheDocument();
    expect(pushMock).not.toHaveBeenCalled();
  });

  it("shows login prompt when posting is disabled", () => {
    render(<CreatePost canPost={false} />);

    expect(screen.getByText(/log in to publish posts/i)).toBeInTheDocument();
  });
});



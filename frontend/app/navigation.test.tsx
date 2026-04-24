import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

const { pathnameState, pushMock } = vi.hoisted(() => ({
  pathnameState: { value: "/posts" },
  pushMock: vi.fn()
}));

vi.mock("next/navigation", () => ({
  usePathname: () => pathnameState.value,
  useRouter: () => ({ push: pushMock })
}));

vi.mock("next/link", () => ({
  default: ({ href, children, ...rest }: { href: string; children: React.ReactNode }) => <a href={href} {...rest}>{children}</a>
}));

vi.mock("@/app/theme-toggle", () => ({
  ThemeToggle: () => <div>theme-toggle</div>
}));

import { Navigation } from "@/app/navigation";

describe("Navigation", () => {
  beforeEach(() => {
    pushMock.mockReset();
    pathnameState.value = "/posts";
    vi.restoreAllMocks();
  });

  it("shows login link for guests except on login page", async () => {
    vi.spyOn(global, "fetch").mockResolvedValue({ ok: false } as Response);

    const { rerender } = render(<Navigation />);

    expect(await screen.findByRole("link", { name: /login/i })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /^posts$/i })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /users/i })).toBeInTheDocument();

    const postsLink = screen.getByRole("link", { name: /^posts$/i });
    expect(postsLink).toHaveClass("is-active");

    pathnameState.value = "/login";
    rerender(<Navigation />);

    await waitFor(() => {
      expect(screen.queryByRole("link", { name: /login/i })).not.toBeInTheDocument();
    });
  });

  it("shows admin menu and logs out", async () => {
    vi.spyOn(global, "fetch")
      .mockResolvedValueOnce({
        ok: true,
        json: vi.fn().mockResolvedValue({
          id: "1",
          login: "admin",
          fullName: "System Admin",
          status: "🟢 Available",
          role: "Admin"
        })
      } as unknown as Response)
      .mockResolvedValueOnce({ ok: true } as Response);

    render(<Navigation />);

    await userEvent.click(await screen.findByTitle("admin"));

    expect(screen.getByRole("link", { name: /dashboard/i })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /manage users/i })).toBeInTheDocument();

    await userEvent.click(document.body);
    await waitFor(() => {
      expect(screen.queryByRole("link", { name: /dashboard/i })).not.toBeInTheDocument();
    });

    await userEvent.click(screen.getByTitle("admin"));

    await userEvent.click(screen.getByRole("button", { name: /logout/i }));

    await waitFor(() => {
      expect(pushMock).toHaveBeenCalledWith("/login");
    });
  });

  it("hides current-route menu links for authenticated users", async () => {
    pathnameState.value = "/dashboard";

    vi.spyOn(global, "fetch").mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue({
        id: "1",
        login: "user",
        fullName: "User Name",
        status: "🟢 Available",
        role: "User"
      })
    } as unknown as Response);

    render(<Navigation />);

    await userEvent.click(await screen.findByTitle("user"));

    expect(screen.queryByRole("link", { name: /dashboard/i })).not.toBeInTheDocument();
    expect(screen.getByRole("link", { name: /my profile/i })).toBeInTheDocument();
  });
});



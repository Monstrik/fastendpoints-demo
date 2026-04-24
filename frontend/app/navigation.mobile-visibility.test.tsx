import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";

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

describe("Navigation mobile auth-control visibility", () => {
  afterEach(() => {
    vi.restoreAllMocks();
    pathnameState.value = "/posts";
    pushMock.mockReset();
  });

  it("hides header login link when mobile nav is open", async () => {
    vi.spyOn(global, "fetch").mockResolvedValue({ ok: false } as Response);

    const { container } = render(<Navigation />);

    await screen.findByRole("link", { name: /login/i });
    expect(container.querySelector(".nav-login-link")).not.toHaveClass("mobile-hidden-while-open");

    await userEvent.click(screen.getByRole("button", { name: /open navigation menu/i }));

    expect(container.querySelector(".nav-login-link")).toHaveClass("mobile-hidden-while-open");
  });

  it("hides header user menu control when mobile nav is open", async () => {
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

    const { container } = render(<Navigation />);

    await screen.findByTitle("user");
    expect(container.querySelector(".user-menu-container")).not.toHaveClass("mobile-hidden-while-open");

    await userEvent.click(screen.getByRole("button", { name: /open navigation menu/i }));

    expect(container.querySelector(".user-menu-container")).toHaveClass("mobile-hidden-while-open");
  });
});


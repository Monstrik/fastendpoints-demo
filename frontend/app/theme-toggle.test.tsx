import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { ThemeToggle } from "@/app/theme-toggle";

describe("ThemeToggle", () => {
  it("loads saved theme and toggles it", async () => {
    localStorage.setItem("theme", "light");

    render(<ThemeToggle />);

    const button = screen.getByRole("switch", { name: /toggle theme/i });

    await waitFor(() => {
      expect(button).toHaveAttribute("aria-checked", "true");
      expect(document.documentElement).toHaveClass("light");
    });

    await userEvent.click(button);

    expect(button).toHaveAttribute("aria-checked", "false");
    expect(localStorage.getItem("theme")).toBe("dark");
    expect(document.documentElement).toHaveClass("dark");
  });
});


import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { LoginForm } from "@/app/login/login-form";

const { replaceMock, refreshMock } = vi.hoisted(() => ({
  replaceMock: vi.fn(),
  refreshMock: vi.fn()
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    replace: replaceMock,
    refresh: refreshMock
  })
}));

describe("LoginForm", () => {
  beforeEach(() => {
    replaceMock.mockReset();
    refreshMock.mockReset();
    vi.restoreAllMocks();
  });

  it("redirects to dashboard on successful login", async () => {
    const fetchMock = vi.spyOn(global, "fetch").mockResolvedValue({
      ok: true
    } as Response);

    render(<LoginForm />);

    await userEvent.type(screen.getByLabelText(/login/i), "aya");
    await userEvent.type(screen.getByLabelText(/password/i), "secret123");
    await userEvent.click(screen.getByRole("button", { name: /login/i }));

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith("/api/auth/login", expect.objectContaining({
        method: "POST",
        credentials: "include"
      }));
      expect(replaceMock).toHaveBeenCalledWith("/dashboard");
      expect(refreshMock).toHaveBeenCalled();
    });
  });

  it("shows backend error on failed login", async () => {
    vi.spyOn(global, "fetch").mockResolvedValue({
      ok: false,
      json: vi.fn().mockResolvedValue({ message: "Invalid credentials." })
    } as unknown as Response);

    render(<LoginForm />);

    await userEvent.type(screen.getByLabelText(/login/i), "aya");
    await userEvent.type(screen.getByLabelText(/password/i), "wrong");
    await userEvent.click(screen.getByRole("button", { name: /login/i }));

    expect(await screen.findByText("Invalid credentials.")).toBeInTheDocument();
    expect(replaceMock).not.toHaveBeenCalled();
  });
});


import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { ForgotPasswordForm } from "@/app/forgot-password/forgot-password-form";

describe("ForgotPasswordForm", () => {
  it("shows success message after submitting", async () => {
    const fetchMock = vi.spyOn(global, "fetch").mockResolvedValue({ ok: true } as Response);

    render(<ForgotPasswordForm />);

    await userEvent.type(screen.getByLabelText(/login/i), "aya");
    await userEvent.click(screen.getByRole("button", { name: /send reset link/i }));

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith("/api/auth/forgot-password", expect.objectContaining({
        method: "POST",
        credentials: "include"
      }));
    });

    expect(await screen.findByText(/if your login exists/i)).toBeInTheDocument();
  });

  it("shows backend error on failure", async () => {
    vi.spyOn(global, "fetch").mockResolvedValue({
      ok: false,
      json: vi.fn().mockResolvedValue({ message: "Nope" })
    } as unknown as Response);

    render(<ForgotPasswordForm />);

    await userEvent.type(screen.getByLabelText(/login/i), "aya");
    await userEvent.click(screen.getByRole("button", { name: /send reset link/i }));

    expect(await screen.findByText("Nope")).toBeInTheDocument();
  });
});


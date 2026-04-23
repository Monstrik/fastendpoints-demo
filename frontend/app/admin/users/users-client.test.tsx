import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AdminUsersClient } from "@/app/admin/users/users-client";

describe("AdminUsersClient", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it("loads users and can create a new user", async () => {
    const fetchMock = vi.spyOn(global, "fetch")
      .mockResolvedValueOnce({
        ok: true,
        json: vi.fn().mockResolvedValue([])
      } as unknown as Response)
      .mockResolvedValueOnce({ ok: true } as Response)
      .mockResolvedValueOnce({
        ok: true,
        json: vi.fn().mockResolvedValue([
          {
            id: "u1",
            login: "aya",
            firstName: "Aya",
            lastName: "Kovi",
            fullName: "Aya Kovi",
            role: "User",
            status: "🟢 Available"
          }
        ])
      } as unknown as Response);

    render(<AdminUsersClient />);

    await screen.findByText("Users");

    await userEvent.type(screen.getByLabelText(/^login$/i), "aya");
    await userEvent.type(screen.getByLabelText(/password/i), "User123!");
    await userEvent.type(screen.getByLabelText(/first name/i), "Aya");
    await userEvent.type(screen.getByLabelText(/last name/i), "Kovi");
    await userEvent.click(screen.getByRole("button", { name: /create/i }));

    await waitFor(() => {
      expect(fetchMock).toHaveBeenNthCalledWith(2, "/api/admin/users", expect.objectContaining({ method: "POST" }));
      expect(screen.getByText("Aya Kovi")).toBeInTheDocument();
    });
  });

  it("shows error when deleting fails", async () => {
    vi.spyOn(global, "fetch")
      .mockResolvedValueOnce({
        ok: true,
        json: vi.fn().mockResolvedValue([
          {
            id: "u1",
            login: "aya",
            firstName: "Aya",
            lastName: "Kovi",
            fullName: "Aya Kovi",
            role: "User",
            status: "🟢 Available"
          }
        ])
      } as unknown as Response)
      .mockResolvedValueOnce({ ok: false } as Response);

    render(<AdminUsersClient />);

    await userEvent.click(await screen.findByRole("button", { name: /delete/i }));

    expect(await screen.findByText("Could not delete user.")).toBeInTheDocument();
  });
});


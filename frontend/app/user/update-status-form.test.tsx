import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { UpdateStatusForm } from "@/app/user/update-status-form";

describe("UpdateStatusForm", () => {
  it("submits status change when selection changes", async () => {
    const fetchMock = vi.spyOn(global, "fetch").mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue({ status: "🎯 Focused" })
    } as unknown as Response);

    render(<UpdateStatusForm currentStatus="🟢 Available" />);

    await userEvent.selectOptions(screen.getByLabelText(/status/i), "🎯 Focused");

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith("/api/me/status", expect.objectContaining({
        method: "PUT",
        credentials: "include"
      }));
    });

    expect(await screen.findByText("Status updated successfully.")).toBeInTheDocument();
  });

  it("reverts selection and shows error when update fails", async () => {
    vi.spyOn(global, "fetch").mockResolvedValue({
      ok: false,
      json: vi.fn().mockResolvedValue({ message: "Could not save" })
    } as unknown as Response);

    render(<UpdateStatusForm currentStatus="🟢 Available" />);

    const select = screen.getByLabelText(/status/i) as HTMLSelectElement;
    await userEvent.selectOptions(select, "🎯 Focused");

    expect(await screen.findByText("Could not save")).toBeInTheDocument();
    expect(select.value).toBe("🟢 Available");
  });
});


import { beforeEach, describe, expect, it, vi } from "vitest";

const { getCookieMock, redirectMock, backendFetchMock } = vi.hoisted(() => ({
  getCookieMock: vi.fn(),
  redirectMock: vi.fn(),
  backendFetchMock: vi.fn()
}));

vi.mock("next/headers", () => ({
  cookies: () => ({
    get: getCookieMock
  })
}));

vi.mock("next/navigation", () => ({
  redirect: redirectMock
}));

vi.mock("@/lib/api", () => ({
  backendFetch: backendFetchMock
}));

import { AUTH_COOKIE, getAuthToken, getCurrentUser, requireAdmin, requireAuth } from "@/lib/auth";

describe("auth helpers", () => {
  beforeEach(() => {
    getCookieMock.mockReset();
    redirectMock.mockReset();
    backendFetchMock.mockReset();
  });

  it("reads auth token from cookies", () => {
    getCookieMock.mockReturnValue({ value: "token-123" });
    expect(getAuthToken()).toBe("token-123");
    expect(getCookieMock).toHaveBeenCalledWith(AUTH_COOKIE);
  });

  it("returns null when there is no current user token", async () => {
    getCookieMock.mockReturnValue(undefined);
    await expect(getCurrentUser()).resolves.toBeNull();
  });

  it("loads current user and enforces auth/admin redirects", async () => {
    getCookieMock.mockReturnValue({ value: "token-123" });
    backendFetchMock.mockResolvedValueOnce({
      ok: true,
      json: vi.fn().mockResolvedValue({
        id: "1",
        login: "aya",
        firstName: "Aya",
        lastName: "Kovi",
        fullName: "Aya Kovi",
        role: "User",
        status: "🟢 Available"
      })
    });

    await expect(getCurrentUser()).resolves.toMatchObject({ login: "aya" });

    backendFetchMock.mockResolvedValueOnce({ ok: false });
    await requireAuth();
    expect(redirectMock).toHaveBeenCalledWith("/login");

    backendFetchMock.mockResolvedValueOnce({
      ok: true,
      json: vi.fn().mockResolvedValue({
        id: "1",
        login: "aya",
        firstName: "Aya",
        lastName: "Kovi",
        fullName: "Aya Kovi",
        role: "User",
        status: "🟢 Available"
      })
    });
    await requireAdmin();
    expect(redirectMock).toHaveBeenCalledWith("/dashboard");
  });
});


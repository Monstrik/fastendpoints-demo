import { afterEach, describe, expect, it, vi } from "vitest";
import { backendFetch } from "@/lib/api";

describe("backendFetch", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("adds auth and json headers when needed", async () => {
    const fetchMock = vi.spyOn(global, "fetch").mockResolvedValue(new Response(null, { status: 200 }));

    await backendFetch("/api/me", {
      method: "POST",
      token: "abc",
      body: JSON.stringify({ ok: true })
    });

    expect(fetchMock).toHaveBeenCalledWith(
      "http://localhost:5116/api/me",
      expect.objectContaining({
        method: "POST",
        cache: "no-store",
        headers: expect.any(Headers)
      })
    );

    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    const headers = init.headers as Headers;
    expect(headers.get("Authorization")).toBe("Bearer abc");
    expect(headers.get("Content-Type")).toBe("application/json");
  });

  it("returns a synthetic 503 response when fetch throws", async () => {
    vi.spyOn(global, "fetch").mockRejectedValue(new Error("offline"));

    const response = await backendFetch("/api/me");

    expect(response.status).toBe(503);
    await expect(response.json()).resolves.toEqual({ message: "Service unavailable. Please try again shortly." });
  });
});


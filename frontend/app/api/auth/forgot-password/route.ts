import { NextResponse } from "next/server";
import { backendFetch } from "@/lib/api";

export async function POST(request: Request) {
  const payload = (await request.json()) as { email?: string; login?: string };

  const backendResponse = await backendFetch("/api/auth/forgot-password", {
    method: "POST",
    body: JSON.stringify(payload)
  });

  if (!backendResponse.ok) {
    const body = (await backendResponse.json().catch(() => null)) as { message?: string } | null;

    return NextResponse.json(
      { message: body?.message ?? "Service unavailable. Please try again shortly." },
      { status: backendResponse.status }
    );
  }

  return NextResponse.json({ ok: true });
}

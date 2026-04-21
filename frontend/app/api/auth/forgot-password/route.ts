import { NextResponse } from "next/server";
import { backendFetch } from "@/lib/api";

export async function POST(request: Request) {
  const payload = (await request.json()) as { email: string };

  const backendResponse = await backendFetch("/api/auth/forgot-password", {
    method: "POST",
    body: JSON.stringify(payload)
  });

  if (!backendResponse.ok) {
    return NextResponse.json(
      { message: "Request failed." },
      { status: backendResponse.status }
    );
  }

  return NextResponse.json({ ok: true });
}

import { NextResponse } from "next/server";
import { AUTH_COOKIE } from "@/lib/auth";
import { backendFetch } from "@/lib/api";

export async function POST(request: Request) {
  const payload = (await request.json()) as { email?: string; login?: string; password: string };

  // Backend uses "login" but client might send "email"
  const loginValue = payload.login || payload.email || "";

  const backendResponse = await backendFetch("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ login: loginValue, password: payload.password })
  });

  const responseBody = await backendResponse.json().catch(() => null);

  if (!backendResponse.ok) {
    return NextResponse.json(
      { message: "Invalid credentials." },
      { status: backendResponse.status }
    );
  }

  const token =
    (responseBody as { token?: string; accessToken?: string } | null)?.token ??
    (responseBody as { token?: string; accessToken?: string } | null)?.accessToken;

  if (!token) {
    return NextResponse.json(
      { message: "Backend response did not include a token." },
      { status: 500 }
    );
  }

  const response = NextResponse.json({ ok: true });
  response.cookies.set({
    name: AUTH_COOKIE,
    value: token,
    httpOnly: true,
    sameSite: "lax",
    secure: process.env.NODE_ENV === "production",
    path: "/"
  });

  return response;
}

import { NextResponse } from "next/server";
import { getAuthToken } from "@/lib/auth";
import { backendFetch } from "@/lib/api";

export async function GET() {
  const backendResponse = await backendFetch("/api/public/posts", {
    method: "GET"
  });

  const body = await backendResponse.text();
  return new NextResponse(body, {
    status: backendResponse.status,
    headers: { "Content-Type": backendResponse.headers.get("Content-Type") ?? "application/json" }
  });
}

export async function POST(request: Request) {
  const token = getAuthToken();

  if (!token) {
    return NextResponse.json({ message: "Unauthorized" }, { status: 401 });
  }

  const payload = await request.text();

  const backendResponse = await backendFetch("/api/posts", {
    method: "POST",
    token,
    body: payload,
    headers: { "Content-Type": "application/json" }
  });

  const body = await backendResponse.text();
  return new NextResponse(body, {
    status: backendResponse.status,
    headers: { "Content-Type": backendResponse.headers.get("Content-Type") ?? "application/json" }
  });
}


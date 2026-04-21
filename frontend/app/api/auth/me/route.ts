import { NextResponse } from "next/server";
import { getAuthToken } from "@/lib/auth";
import { backendFetch } from "@/lib/api";

export async function GET() {
  const token = getAuthToken();

  if (!token) {
    return NextResponse.json({ message: "Unauthorized" }, { status: 401 });
  }

  const backendResponse = await backendFetch("/api/auth/me", {
    method: "GET",
    token
  });

  if (!backendResponse.ok) {
    return NextResponse.json({ message: "Unauthorized" }, { status: 401 });
  }

  const user = await backendResponse.json();
  return NextResponse.json(user);
}

import { NextResponse } from "next/server";
import { getAuthToken } from "@/lib/auth";
import { backendFetch } from "@/lib/api";

export async function GET() {
  const token = getAuthToken();

  if (!token) {
    return NextResponse.json(null, { status: 401 });
  }

  const response = await backendFetch("/api/me", {
    method: "GET",
    token
  });

  if (!response.ok) {
    return NextResponse.json(null, { status: 401 });
  }

  const data = await response.json();
  return NextResponse.json(data);
}


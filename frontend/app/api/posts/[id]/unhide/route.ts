import { NextResponse } from "next/server";
import { getAuthToken } from "@/lib/auth";
import { backendFetch } from "@/lib/api";

export async function PUT(
  request: Request,
  { params }: { params: { id: string } }
) {
  const token = getAuthToken();

  if (!token) {
    return NextResponse.json({ message: "Unauthorized" }, { status: 401 });
  }

  const backendResponse = await backendFetch(`/api/posts/${params.id}/unhide`, {
    method: "PUT",
    token,
    body: "{}",
    headers: { "Content-Type": "application/json" }
  });

  const body = await backendResponse.text();
  return new NextResponse(body, {
    status: backendResponse.status,
    headers: { "Content-Type": backendResponse.headers.get("Content-Type") ?? "application/json" }
  });
}


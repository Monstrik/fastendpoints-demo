import { NextResponse } from "next/server";
import { getAuthToken } from "@/lib/auth";
import { backendFetch } from "@/lib/api";

export async function DELETE(
  request: Request,
  { params }: { params: { id: string } }
) {
  const token = getAuthToken();

  if (!token) {
    return NextResponse.json({ message: "Unauthorized" }, { status: 401 });
  }

  const backendResponse = await backendFetch(`/api/users/${params.id}`, {
    method: "DELETE",
    token
  });

  if (backendResponse.status === 204) {
    return new NextResponse(null, { status: 204 });
  }

  const body = await backendResponse.text();
  return new NextResponse(body, {
    status: backendResponse.status,
    headers: { "Content-Type": backendResponse.headers.get("Content-Type") ?? "application/json" }
  });
}

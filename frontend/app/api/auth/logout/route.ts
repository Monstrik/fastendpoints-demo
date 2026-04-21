import { NextResponse } from "next/server";
import { AUTH_COOKIE } from "@/lib/auth";

export async function POST(request: Request) {
  const response = NextResponse.redirect(new URL("/login", request.url));

  response.cookies.set({
    name: AUTH_COOKIE,
    value: "",
    path: "/",
    expires: new Date(0)
  });

  return response;
}

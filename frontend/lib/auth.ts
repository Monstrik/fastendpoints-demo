import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { backendFetch } from "@/lib/api";

export const AUTH_COOKIE = "auth_token";

export type AuthUser = {
  id: string;
  login: string;
  firstName: string;
  lastName: string;
  fullName: string;
  role: string;
  status: string;
};

export function getAuthToken() {
  return cookies().get(AUTH_COOKIE)?.value;
}

export async function getCurrentUser(): Promise<AuthUser | null> {
  const token = getAuthToken();

  if (!token) {
    return null;
  }

  const response = await backendFetch("/api/me", {
    method: "GET",
    token
  });

  if (!response.ok) {
    return null;
  }

  return (await response.json()) as AuthUser;
}

export async function requireAuth() {
  const user = await getCurrentUser();

  if (!user) {
    redirect("/login");
  }

  return user;
}

export async function requireAdmin() {
  const user = await requireAuth();

  if (user.role.toLowerCase() !== "admin") {
    redirect("/dashboard");
  }

  return user;
}

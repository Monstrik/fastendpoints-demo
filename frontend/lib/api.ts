const BACKEND_URL = process.env.BACKEND_URL ?? "http://localhost:5116";

export type BackendRequestInit = RequestInit & {
  token?: string;
};

export async function backendFetch(path: string, init: BackendRequestInit = {}) {
  const headers = new Headers(init.headers);

  if (!headers.has("Content-Type") && init.body) {
    headers.set("Content-Type", "application/json");
  }

  if (init.token) {
    headers.set("Authorization", `Bearer ${init.token}`);
  }

  return fetch(`${BACKEND_URL}${path}`, {
    ...init,
    headers,
    cache: "no-store"
  });
}


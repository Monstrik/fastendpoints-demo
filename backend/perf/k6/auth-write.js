import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  scenarios: {
    auth_write: {
      executor: "constant-vus",
      vus: Number(__ENV.VUS || 10),
      duration: __ENV.DURATION || "30s",
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<600"],
  },
};

const baseUrl = __ENV.BASE_URL || "http://localhost:5116";

export function setup() {
  const loginPayload = JSON.stringify({
    login: __ENV.ADMIN_LOGIN || "admin",
    password: __ENV.ADMIN_PASSWORD || "Admin123!",
  });

  const loginRes = http.post(`${baseUrl}/api/auth/login`, loginPayload, {
    headers: { "Content-Type": "application/json" },
  });

  check(loginRes, {
    "login returns 200": (r) => r.status === 200,
  });

  const body = loginRes.json();
  return { token: body?.token || "" };
}

export default function (data) {
  const headers = {
    Authorization: `Bearer ${data.token}`,
    "Content-Type": "application/json",
    Accept: "application/json",
  };

  const createPostRes = http.post(
    `${baseUrl}/api/posts`,
    JSON.stringify({ content: `load test post ${Date.now()}` }),
    { headers }
  );

  check(createPostRes, {
    "create post returns 201": (r) => r.status === 201,
  });

  sleep(0.2);
}


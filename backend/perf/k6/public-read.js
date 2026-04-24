import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  scenarios: {
    public_read: {
      executor: "constant-vus",
      vus: Number(__ENV.VUS || 20),
      duration: __ENV.DURATION || "30s",
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<400"],
  },
};

const baseUrl = __ENV.BASE_URL || "http://localhost:5116";

export default function () {
  const posts = http.get(`${baseUrl}/api/public/posts`, {
    headers: { Accept: "application/json" },
  });
  check(posts, {
    "public posts returns 200": (r) => r.status === 200,
  });

  const users = http.get(`${baseUrl}/api/public/users`, {
    headers: { Accept: "application/json" },
  });
  check(users, {
    "public users returns 200": (r) => r.status === 200,
  });

  sleep(0.2);
}


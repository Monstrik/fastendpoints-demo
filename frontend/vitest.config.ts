import path from "node:path";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, ".")
    }
  },
  test: {
    environment: "jsdom",
    setupFiles: ["./test/setup.ts"],
    coverage: {
      provider: "v8",
      reporter: ["text", "html", "lcov"],
      exclude: [
        "test/**",
        "**/*.test.{ts,tsx}",
        ".next/**",
        "node_modules/**",
        "next-env.d.ts",
        "tailwind.config.js",
        "postcss.config.js",
        "next.config.js"
      ]
    }
  }
});


"use client";

import { useEffect, useState } from "react";

type Theme = "dark" | "light";

function applyTheme(theme: Theme) {
  const root = document.documentElement;
  root.classList.remove("dark", "light");
  root.classList.add(theme);
}

export function ThemeToggle() {
  const [theme, setTheme] = useState<Theme>("dark");

  useEffect(() => {
    const stored = localStorage.getItem("theme");
    const nextTheme: Theme = stored === "light" ? "light" : "dark";
    setTheme(nextTheme);
    applyTheme(nextTheme);
  }, []);

  function toggleTheme() {
    const nextTheme: Theme = theme === "dark" ? "light" : "dark";
    setTheme(nextTheme);
    localStorage.setItem("theme", nextTheme);
    applyTheme(nextTheme);
  }

  const isLight = theme === "light";

  return (
    <button
      type="button"
      className="theme-switch"
      role="switch"
      aria-checked={isLight}
      aria-label="Toggle theme"
      onClick={toggleTheme}
      title={isLight ? "Switch to dark mode" : "Switch to light mode"}
    >
      <span className={`theme-switch-track ${isLight ? "is-light" : "is-dark"}`}>
        <span className="theme-switch-thumb">{isLight ? "☀️" : "🌙"}</span>
      </span>
    </button>
  );
}

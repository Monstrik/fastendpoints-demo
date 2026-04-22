import type { ReactNode } from "react";
import "./globals.css";
import { ThemeToggle } from "@/app/theme-toggle";

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en" className="dark" suppressHydrationWarning>
      <body>
        <header>
          <ThemeToggle />
        </header>
        <main>{children}</main>
      </body>
    </html>
  );
}

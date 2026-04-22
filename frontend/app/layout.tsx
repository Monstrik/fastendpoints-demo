import type { ReactNode } from "react";
import "./globals.css";
import { ThemeToggle } from "@/app/theme-toggle";
import { Navigation } from "@/app/navigation";

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en" className="dark" suppressHydrationWarning>
      <body>
        <header>
          <Navigation />
          <ThemeToggle />
        </header>
        <main>{children}</main>
      </body>
    </html>
  );
}

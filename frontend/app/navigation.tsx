"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState, useRef } from "react";
import { ThemeToggle } from "@/app/theme-toggle";

interface User {
  id: string;
  login: string;
  fullName: string;
  status: string;
  role: string;
}

export function Navigation() {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [menuOpen, setMenuOpen] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const router = useRouter();
  const pathname = usePathname();
  const menuRef = useRef<HTMLDivElement>(null);
  const mobileMenuRef = useRef<HTMLDivElement>(null);
  const normalizedPath = pathname?.replace(/\/$/, "") || "/";
  const isLoginRoute = normalizedPath === "/login";
  const isCurrentRoute = (href: string) => {
    const normalizedHref = href.replace(/\/$/, "") || "/";
    return normalizedPath === normalizedHref;
  };
  const navLinkClass = (href: string) => (isCurrentRoute(href) ? "is-active" : undefined);

  useEffect(() => {
    const fetchUser = async () => {
      setLoading(true);
      try {
        const response = await fetch("/api/me", {
          method: "GET",
          credentials: "include",
          cache: "no-store"
        });
        if (response.ok) {
          const currentUser = await response.json();
          setUser(currentUser);
        } else {
          setUser(null);
        }
      } catch (error) {
        console.error("Failed to fetch user:", error);
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    fetchUser();
  }, [pathname]);

  useEffect(() => {
    setMenuOpen(false);
    setMobileMenuOpen(false);
  }, [pathname]);

  // Close menus when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setMenuOpen(false);
      }

      if (mobileMenuRef.current && !mobileMenuRef.current.contains(event.target as Node)) {
        setMobileMenuOpen(false);
      }
    };

    if (menuOpen || mobileMenuOpen) {
      document.addEventListener("mousedown", handleClickOutside);
      return () => document.removeEventListener("mousedown", handleClickOutside);
    }
  }, [menuOpen, mobileMenuOpen]);

  const toggleUserMenu = () => {
    setMenuOpen((current) => !current);
    setMobileMenuOpen(false);
  };

  const toggleMobileMenu = () => {
    setMobileMenuOpen((current) => !current);
    setMenuOpen(false);
  };

  const handleLogout = async () => {
    const response = await fetch("/api/auth/logout", { method: "POST" });
    if (response.ok) {
      setUser(null);
      setMenuOpen(false);
      setMobileMenuOpen(false);
      router.push("/login");
    }
  };

  if (loading) {
    return null;
  }

  return (
    <nav className="nav-menu">
      <ul className="nav-links nav-left">
        <li className="nav-brand-item">
          <Link href="/posts" className="nav-brand-link" aria-label="MyWebApp home">
            <img src="/branding/FED-LOGO.png" alt="MyWebApp logo" className="nav-brand-logo" />
          </Link>
        </li>
        <li className="desktop-nav-item">
          <Link href="/posts" className={navLinkClass("/posts")}>Posts</Link>
        </li>
        <li className="desktop-nav-item">
          <Link href="/users" className={navLinkClass("/users")}>Users</Link>
        </li>
      </ul>
      <div className="nav-right-container" ref={mobileMenuRef}>
        {user ? (
          <div className={`user-menu-container user-menu-desktop-only ${mobileMenuOpen ? "mobile-hidden-while-open" : ""}`.trim()} ref={menuRef}>
            <button
              className="user-icon-button"
              onClick={toggleUserMenu}
              title={user.login}
              aria-expanded={menuOpen}
              aria-controls="user-menu-panel"
            >
              <span className="user-icon">👤</span>
            </button>
            {menuOpen && (
              <div className="user-dropdown-menu" id="user-menu-panel">
                <div className="menu-header">
                  <span className="menu-username">{user.login}</span>
                </div>
                <ul className="menu-items">
                  <li>
                    <Link href="/dashboard" className={navLinkClass("/dashboard")} onClick={() => setMenuOpen(false)}>
                      Dashboard
                    </Link>
                  </li>
                  <li>
                    <Link href="/user" className={navLinkClass("/user")} onClick={() => setMenuOpen(false)}>
                      My Profile
                    </Link>
                  </li>
                  {user.role.toLowerCase() === "admin" && (
                    <li>
                      <Link href="/admin/users" className={navLinkClass("/admin/users")} onClick={() => setMenuOpen(false)}>
                        Manage Users
                      </Link>
                    </li>
                  )}
                  <li className="menu-divider"></li>
                  <li className="theme-toggle-item">
                    <span>Theme</span>
                    <ThemeToggle />
                  </li>
                  <li>
                    <button onClick={handleLogout} className="logout-button">
                      Logout
                    </button>
                  </li>
                </ul>
              </div>
            )}
          </div>
        ) : (
          !isLoginRoute && (
            <Link href="/login" className={`nav-login-link ${mobileMenuOpen ? "mobile-hidden-while-open" : ""}`.trim()}>
              Login
            </Link>
          )
        )}

        <button
          type="button"
          className="mobile-nav-toggle"
          aria-label={mobileMenuOpen ? "Close navigation menu" : "Open navigation menu"}
          aria-expanded={mobileMenuOpen}
          aria-controls="mobile-navigation-panel"
          onClick={toggleMobileMenu}
        >
          <span aria-hidden="true">{mobileMenuOpen ? "✕" : "☰"}</span>
        </button>

        {mobileMenuOpen && (
          <div className="mobile-nav-panel" id="mobile-navigation-panel" aria-label="Mobile navigation">
            <div className="mobile-nav-section">
              <Link href="/posts" className={navLinkClass("/posts")} onClick={() => setMobileMenuOpen(false)}>
                Posts
              </Link>
              <Link href="/users" className={navLinkClass("/users")} onClick={() => setMobileMenuOpen(false)}>
                Users
              </Link>
            </div>

            {user ? (
              <>
                <div className="mobile-nav-divider" />
                <div className="mobile-nav-section">
                  <Link href="/dashboard" className={navLinkClass("/dashboard")} onClick={() => setMobileMenuOpen(false)}>
                    Dashboard
                  </Link>
                  <Link href="/user" className={navLinkClass("/user")} onClick={() => setMobileMenuOpen(false)}>
                    My Profile
                  </Link>
                  {user.role.toLowerCase() === "admin" && (
                    <Link href="/admin/users" className={navLinkClass("/admin/users")} onClick={() => setMobileMenuOpen(false)}>
                      Manage Users
                    </Link>
                  )}
                </div>
                <div className="mobile-nav-divider" />
                <div className="mobile-nav-theme-row">
                  <span>Theme</span>
                  <ThemeToggle />
                </div>
                <button type="button" onClick={handleLogout} className="logout-button mobile-logout-button">
                  Logout
                </button>
              </>
            ) : (
              !isLoginRoute && (
                <>
                  <div className="mobile-nav-divider" />
                  <div className="mobile-nav-section">
                    <Link href="/login" onClick={() => setMobileMenuOpen(false)}>
                      Login
                    </Link>
                  </div>
                </>
              )
            )}
          </div>
        )}
      </div>
    </nav>
  );
}


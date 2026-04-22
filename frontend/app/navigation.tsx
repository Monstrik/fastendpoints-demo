"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
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
  const router = useRouter();
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const fetchUser = async () => {
      try {
        const response = await fetch("/api/me");
        if (response.ok) {
          const currentUser = await response.json();
          setUser(currentUser);
        }
      } catch (error) {
        console.error("Failed to fetch user:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchUser();
  }, []);

  // Close menu when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setMenuOpen(false);
      }
    };

    if (menuOpen) {
      document.addEventListener("mousedown", handleClickOutside);
      return () => document.removeEventListener("mousedown", handleClickOutside);
    }
  }, [menuOpen]);

  const handleLogout = async () => {
    const response = await fetch("/api/auth/logout", { method: "POST" });
    if (response.ok) {
      setUser(null);
      setMenuOpen(false);
      router.push("/login");
    }
  };

  if (loading) {
    return null;
  }

  return (
    <nav className="nav-menu">
      <ul className="nav-links nav-left">
        <li>
          <Link href="/users">View Users</Link>
        </li>
      </ul>
      <div className="nav-right-container">
        {user ? (
          <div className="user-menu-container" ref={menuRef}>
            <button
              className="user-icon-button"
              onClick={() => setMenuOpen(!menuOpen)}
              title={user.login}
            >
              <span className="user-icon">👤</span>
            </button>
            {menuOpen && (
              <div className="user-dropdown-menu">
                <div className="menu-header">
                  <span className="menu-username">{user.login}</span>
                </div>
                <ul className="menu-items">
                  <li>
                    <Link href="/dashboard" onClick={() => setMenuOpen(false)}>
                      Dashboard
                    </Link>
                  </li>
                  <li>
                    <Link href="/user" onClick={() => setMenuOpen(false)}>
                      My Profile
                    </Link>
                  </li>
                  {user.role.toLowerCase() === "admin" && (
                    <li>
                      <Link href="/admin/users" onClick={() => setMenuOpen(false)}>
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
          <Link href="/login" className="nav-login-link">
            Login
          </Link>
        )}
      </div>
    </nav>
  );
}


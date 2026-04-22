"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";

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
  const router = useRouter();

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

  const handleLogout = async () => {
    const response = await fetch("/api/auth/logout", { method: "POST" });
    if (response.ok) {
      setUser(null);
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
      <ul className="nav-links nav-right">
        {user ? (
          <>
            <li className="nav-user-info">
              <span className="nav-username">{user.login}</span>
            </li>
            <li>
              <Link href="/dashboard">Dashboard</Link>
            </li>
            <li>
              <Link href="/user">My Profile</Link>
            </li>
            {user.role.toLowerCase() === "admin" && (
              <li>
                <Link href="/admin/users">Manage Users</Link>
              </li>
            )}
            <li>
              <button onClick={handleLogout} className="nav-logout">
                Logout
              </button>
            </li>
          </>
        ) : (
          <li>
            <Link href="/login">Login</Link>
          </li>
        )}
      </ul>
    </nav>
  );
}


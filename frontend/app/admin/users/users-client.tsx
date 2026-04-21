"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import type { AdminUser } from "@/lib/types";

export function AdminUsersClient() {
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [login, setLogin] = useState("");
  const [password, setPassword] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [age, setAge] = useState(30);
  const [role, setRole] = useState(1); // 0 = Admin, 1 = User
  const [error, setError] = useState<string | null>(null);

  const fetchUsers = useCallback(async () => {
    setLoading(true);
    try {
      const response = await fetch("/api/admin/users", { credentials: "include" });
      if (response.ok) {
        const data = (await response.json()) as AdminUser[];
        setUsers(data);
      }
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  async function onAddUser(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    const response = await fetch("/api/admin/users", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
      body: JSON.stringify({ login, password, firstName, lastName, age, role })
    });

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as { message?: string; errors?: Record<string, string[]> } | null;
      const errorMsg = body?.message || Object.values(body?.errors || {}).flat()[0] || "Could not create user.";
      setError(errorMsg);
      return;
    }

    setLogin("");
    setPassword("");
    setFirstName("");
    setLastName("");
    setAge(30);
    setRole(1);
    await fetchUsers();
  }

  async function onDeleteUser(id: string) {
    setError(null);

    const response = await fetch(`/api/admin/users/${id}`, {
      method: "DELETE",
      credentials: "include"
    });

    if (!response.ok) {
      setError("Could not delete user.");
      return;
    }

    await fetchUsers();
  }

  return (
    <div>
      <form onSubmit={onAddUser}>
        <h2>Add User</h2>

        <label htmlFor="login">Login</label>
        <input
          id="login"
          type="text"
          value={login}
          onChange={(event) => setLogin(event.target.value)}
          required
        />

        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          required
        />

        <label htmlFor="firstName">First Name</label>
        <input
          id="firstName"
          type="text"
          value={firstName}
          onChange={(event) => setFirstName(event.target.value)}
          required
        />

        <label htmlFor="lastName">Last Name</label>
        <input
          id="lastName"
          type="text"
          value={lastName}
          onChange={(event) => setLastName(event.target.value)}
          required
        />

        <label htmlFor="age">Age</label>
        <input
          id="age"
          type="number"
          value={age}
          onChange={(event) => setAge(parseInt(event.target.value, 10))}
          required
        />

        <label htmlFor="role">Role</label>
        <select id="role" value={role} onChange={(event) => setRole(parseInt(event.target.value, 10))}>
          <option value={0}>Admin</option>
          <option value={1}>User</option>
        </select>

        <button type="submit">Create</button>
      </form>

      {error ? <p style={{ color: "red" }}>{error}</p> : null}

      <h2>Users</h2>
      {loading ? (
        <p>Loading...</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Login</th>
              <th>Name</th>
              <th>Age</th>
              <th>Role</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id}>
                <td>{user.login}</td>
                <td>{user.fullName}</td>
                <td>{user.age}</td>
                <td>{user.role}</td>
                <td>
                  <button type="button" onClick={() => onDeleteUser(user.id)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

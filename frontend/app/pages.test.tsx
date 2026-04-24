import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const { redirectMock, getCurrentUserMock, requireAuthMock, requireAdminMock, getAuthTokenMock, backendFetchMock } = vi.hoisted(() => ({
  redirectMock: vi.fn(),
  getCurrentUserMock: vi.fn(),
  requireAuthMock: vi.fn(),
  requireAdminMock: vi.fn(),
  getAuthTokenMock: vi.fn(),
  backendFetchMock: vi.fn()
}));

vi.mock("next/navigation", () => ({
  redirect: redirectMock
}));

vi.mock("next/link", () => ({
  default: ({ href, children }: { href: string; children: React.ReactNode }) => <a href={href}>{children}</a>
}));

vi.mock("@/lib/auth", () => ({
  getCurrentUser: getCurrentUserMock,
  requireAuth: requireAuthMock,
  requireAdmin: requireAdminMock,
  getAuthToken: getAuthTokenMock
}));

vi.mock("@/lib/api", () => ({
  backendFetch: backendFetchMock
}));

vi.mock("@/app/navigation", () => ({
  Navigation: () => <div>navigation</div>
}));

vi.mock("@/app/login/login-form", () => ({
  LoginForm: () => <div>login-form</div>
}));

vi.mock("@/app/forgot-password/forgot-password-form", () => ({
  ForgotPasswordForm: () => <div>forgot-password-form</div>
}));

vi.mock("@/app/posts/create/create-post", () => ({
  CreatePost: ({ canPost }: { canPost: boolean }) => <div>create-post-{String(canPost)}</div>
}));

vi.mock("@/app/user/update-status-form", () => ({
  UpdateStatusForm: ({ currentStatus }: { currentStatus: string }) => <div>update-status-{currentStatus}</div>
}));

vi.mock("@/app/admin/users/users-client", () => ({
  AdminUsersClient: () => <div>admin-users-client</div>
}));

vi.mock("@/app/posts/post-card", () => ({
  PostCard: ({ post }: { post: { content: string } }) => <div>post-card-{post.content}</div>
}));

vi.mock("@/app/posts/posts-feed-client", () => ({
  PostsFeedClient: ({ initialPosts, canModerate, canReact }: { initialPosts: unknown[]; canModerate: boolean; canReact: boolean }) => (
    <div>posts-feed-{initialPosts.length}-{String(canModerate)}-{String(canReact)}</div>
  )
}));

import RootLayout from "@/app/layout";
import HomePage from "@/app/page";
import LoginPage from "@/app/login/page";
import ForgotPasswordPage from "@/app/forgot-password/page";
import UsersStatusPage from "@/app/users/page";
import PostsPage from "@/app/posts/page";
import PostsClientPage from "@/app/posts/create/page";
import UserPage from "@/app/user/page";
import AdminUsersPage from "@/app/admin/users/page";
import DashboardPage from "@/app/dashboard/page";

describe("app pages", () => {
  beforeEach(() => {
    redirectMock.mockReset();
    getCurrentUserMock.mockReset();
    requireAuthMock.mockReset();
    requireAdminMock.mockReset();
    getAuthTokenMock.mockReset();
    backendFetchMock.mockReset();
  });

  it("renders the root layout with navigation and children", () => {
    render(<RootLayout><div>child-content</div></RootLayout>);
    expect(screen.getByText("navigation")).toBeInTheDocument();
    expect(screen.getByText("child-content")).toBeInTheDocument();
  });

  it("redirects the home page to posts", async () => {
    await HomePage();
    expect(redirectMock).toHaveBeenCalledWith("/posts");
  });

  it("redirects authenticated users away from the login page", async () => {
    getCurrentUserMock.mockResolvedValue({ id: "1" });
    await LoginPage();
    expect(redirectMock).toHaveBeenCalledWith("/dashboard");
  });

  it("renders login and forgot-password pages for guests", async () => {
    getCurrentUserMock.mockResolvedValue(null);
    render(await LoginPage());
    expect(screen.getByText("Login")).toBeInTheDocument();
    expect(screen.getByText("login-form")).toBeInTheDocument();

    render(<ForgotPasswordPage />);
    expect(screen.getByText("Forgot Password")).toBeInTheDocument();
    expect(screen.getByText("forgot-password-form")).toBeInTheDocument();
  });

  it("renders users page with fetched users", async () => {
    backendFetchMock.mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue([
        { login: "aya", fullName: "Aya Kovi", status: "🟢 Available" }
      ])
    });

    render(await UsersStatusPage());

    expect(screen.getByText("Users")).toBeInTheDocument();
    expect(screen.getByText("Aya Kovi")).toBeInTheDocument();
  });

  it("renders users page empty state when there are no users", async () => {
    backendFetchMock.mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue([])
    });

    render(await UsersStatusPage());

    expect(screen.getByRole("heading", { name: /no user statuses yet/i })).toBeInTheDocument();
    expect(screen.getByText(/invite teammates to sign in and update their status/i)).toBeInTheDocument();
  });

  it("renders users page error state when fetch fails", async () => {
    backendFetchMock.mockResolvedValue({ ok: false, status: 503 });

    render(await UsersStatusPage());

    expect(screen.getByText(/could not load user statuses/i)).toBeInTheDocument();
  });

  it("renders posts page for guest users", async () => {
    getCurrentUserMock.mockResolvedValue(null);
    getAuthTokenMock.mockReturnValue(undefined);
    backendFetchMock.mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue([
        {
          id: "p1",
          authorLogin: "aya",
          authorStatus: "🟢 Available",
          content: "hello",
          createdAtUtc: "2026-04-23T10:00:00Z",
          likesCount: 0,
          dislikesCount: 0,
          viewerReaction: null
        }
      ])
    });

    render(await PostsPage());
    expect(screen.getByText("Posts")).toBeInTheDocument();
    expect(screen.getByText("posts-feed-1-false-false")).toBeInTheDocument();
  });

  it("renders posts page empty state when there are no posts", async () => {
    getCurrentUserMock.mockResolvedValue(null);
    getAuthTokenMock.mockReturnValue(undefined);
    backendFetchMock.mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue([])
    });

    render(await PostsPage());

    expect(screen.getByText("posts-feed-0-false-false")).toBeInTheDocument();
  });

  it("renders admin posts page with moderation enabled", async () => {
    getCurrentUserMock.mockResolvedValue({ id: "1", role: "Admin" });
    getAuthTokenMock.mockReturnValue("token-123");
    backendFetchMock
      .mockResolvedValueOnce({
        ok: true,
        json: vi.fn().mockResolvedValue([])
      })
      .mockResolvedValueOnce({
        ok: true,
        json: vi.fn().mockResolvedValue([
          {
            id: "p1",
            authorLogin: "aya",
            authorStatus: "🟢 Available",
            content: "admin-view",
            createdAtUtc: "2026-04-23T10:00:00Z",
            isHidden: true,
            likesCount: 0,
            dislikesCount: 0,
            viewerReaction: null
          }
        ])
      });

    render(await PostsPage());

    expect(screen.getByText("All Posts")).toBeInTheDocument();
    expect(screen.getByText("posts-feed-1-true-true")).toBeInTheDocument();
  });

  it("renders dashboard error when auth token for posts is missing", async () => {
    requireAuthMock.mockResolvedValue({
      id: "1",
      login: "aya",
      fullName: "Aya Kovi",
      role: "User",
      status: "🟢 Available"
    });
    getAuthTokenMock.mockReturnValue(undefined);

    render(await DashboardPage());

    expect(screen.getByText(/could not load your posts/i)).toBeInTheDocument();
  });

  it("renders create-post, profile, admin, and dashboard pages", async () => {
    getCurrentUserMock.mockResolvedValue({ id: "1" });
    requireAuthMock.mockResolvedValue({
      id: "1",
      login: "aya",
      fullName: "Aya Kovi",
      role: "User",
      status: "🟢 Available"
    });
    requireAdminMock.mockResolvedValue({ id: "1" });
    getAuthTokenMock.mockReturnValue("token");
    backendFetchMock.mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue([
        {
          id: "p1",
          authorLogin: "aya",
          authorStatus: "🟢 Available",
          content: "hello",
          createdAtUtc: "2026-04-23T10:00:00Z",
          isHidden: false,
          likesCount: 0,
          dislikesCount: 0,
          viewerReaction: null
        }
      ])
    });

    render(await PostsClientPage());
    expect(screen.getByText("create-post-true")).toBeInTheDocument();

    render(await UserPage());
    expect(screen.getByText("My Profile")).toBeInTheDocument();
    expect(screen.getByText("update-status-🟢 Available")).toBeInTheDocument();

    render(await AdminUsersPage());
    expect(screen.getByText("admin-users-client")).toBeInTheDocument();

    render(await DashboardPage());
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(screen.getByText("post-card-hello")).toBeInTheDocument();
  });
});



import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import AppSidebar from "./AppSidebar";

const authMocks = vi.hoisted(() => ({
  useAuth: vi.fn(),
}));

vi.mock("../../features/auth/useAuth", () => ({
  useAuth: authMocks.useAuth,
}));

describe("AppSidebar role filtering", () => {
  it("hides approvals for employee", () => {
    authMocks.useAuth.mockReturnValue({
      user: { id: "u1", role: "Employee" },
    });

    render(
      <MemoryRouter initialEntries={["/dashboard"]}>
        <AppSidebar mobileOpen={false} onMobileClose={() => {}} isMobile={false} />
      </MemoryRouter>,
    );

    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(screen.getByText("Requests")).toBeInTheDocument();
    expect(screen.queryByText("Approvals")).not.toBeInTheDocument();
    expect(screen.queryByText("Admin")).not.toBeInTheDocument();
  });

  it("shows approvals for manager and admin for admin", () => {
    authMocks.useAuth.mockReturnValue({
      user: { id: "u2", role: "Admin" },
    });

    render(
      <MemoryRouter initialEntries={["/dashboard"]}>
        <AppSidebar mobileOpen={false} onMobileClose={() => {}} isMobile={false} />
      </MemoryRouter>,
    );

    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(screen.getByText("Requests")).toBeInTheDocument();
    expect(screen.getByText("Approvals")).toBeInTheDocument();
    expect(screen.getByText("Admin")).toBeInTheDocument();
  });
});

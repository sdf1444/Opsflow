import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import ManagerOrAdminRoute from "./ManagerOrAdminRoute";

const authMocks = vi.hoisted(() => ({
  useAuth: vi.fn(),
}));

vi.mock("../features/auth/useAuth", () => ({
  useAuth: authMocks.useAuth,
}));

describe("ManagerOrAdminRoute", () => {
  it("redirects employee to dashboard", () => {
    authMocks.useAuth.mockReturnValue({
      user: { id: "u1", role: "Employee" },
    });

    render(
      <MemoryRouter initialEntries={["/approvals"]}>
        <Routes>
          <Route
            path="/approvals"
            element={
              <ManagerOrAdminRoute>
                <div>Approvals Page</div>
              </ManagerOrAdminRoute>
            }
          />
          <Route path="/dashboard" element={<div>Dashboard Page</div>} />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText("Dashboard Page")).toBeInTheDocument();
    expect(screen.queryByText("Approvals Page")).not.toBeInTheDocument();
  });

  it("allows manager", () => {
    authMocks.useAuth.mockReturnValue({
      user: { id: "u2", role: "Manager" },
    });

    render(
      <MemoryRouter initialEntries={["/approvals"]}>
        <Routes>
          <Route
            path="/approvals"
            element={
              <ManagerOrAdminRoute>
                <div>Approvals Page</div>
              </ManagerOrAdminRoute>
            }
          />
          <Route path="/dashboard" element={<div>Dashboard Page</div>} />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText("Approvals Page")).toBeInTheDocument();
  });
});

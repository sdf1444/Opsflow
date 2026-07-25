import type { ReactElement } from "react";
import { renderWithProviders } from "./render";
import { buildUser } from "./dataBuilders";

export function renderAsEmployee(ui: ReactElement, route = "/") {
  return renderWithProviders(ui, {
    route,
    user: buildUser({ role: "Employee" }),
  });
}

export function renderAsManager(ui: ReactElement, route = "/") {
  return renderWithProviders(ui, {
    route,
    user: buildUser({ id: "manager-1", role: "Manager", name: "Manager User", email: "manager@example.com" }),
  });
}

export function renderAsAdmin(ui: ReactElement, route = "/") {
  return renderWithProviders(ui, {
    route,
    user: buildUser({ id: "admin-1", role: "Admin", name: "Admin User", email: "admin@example.com" }),
  });
}

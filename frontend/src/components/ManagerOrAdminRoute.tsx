import { Navigate } from "react-router-dom";
import type { PropsWithChildren } from "react";
import { useAuth } from "../features/auth/useAuth";

export default function ManagerOrAdminRoute({ children }: PropsWithChildren) {
  const { user } = useAuth();

  if (!user) {
    return <Navigate to="/dashboard" replace />;
  }

  if (user.role !== "Manager" && user.role !== "Admin") {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
}

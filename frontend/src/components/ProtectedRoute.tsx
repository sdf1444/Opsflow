import { Navigate } from "react-router-dom";
import type { PropsWithChildren } from "react";
import { useAuth } from "../contexts/useAuth";

export default function ProtectedRoute({ children }: PropsWithChildren) {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}

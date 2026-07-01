import { Navigate, Route, Routes } from "react-router-dom";
import ProtectedRoute from "../components/ProtectedRoute";
import MainLayout from "../layouts/MainLayout";
import AdminPage from "../pages/AdminPage";
import ApprovalsPage from "../pages/ApprovalsPage";
import DashboardPage from "../pages/DashboardPage";
import LoginPage from "../pages/LoginPage";
import NotFoundPage from "../pages/NotFoundPage";
import ProfilePage from "../pages/ProfilePage";
import RequestDetailPage from "../pages/RequestDetailPage";
import RequestsPage from "../pages/RequestsPage";

export default function AppRouter() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route
        path="/"
        element={(
          <ProtectedRoute>
            <MainLayout />
          </ProtectedRoute>
        )}
      >
        <Route index element={<Navigate to="dashboard" replace />} />
        <Route path="dashboard" element={<DashboardPage />} />
        <Route path="requests" element={<RequestsPage />} />
        <Route path="requests/:id" element={<RequestDetailPage />} />
        <Route path="approvals" element={<ApprovalsPage />} />
        <Route path="admin" element={<AdminPage />} />
        <Route path="profile" element={<ProfilePage />} />
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}

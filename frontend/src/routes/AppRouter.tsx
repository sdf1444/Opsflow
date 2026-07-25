import { Navigate, Route, Routes } from "react-router-dom";
import ManagerOrAdminRoute from "../components/ManagerOrAdminRoute";
import ProtectedRoute from "../components/ProtectedRoute";
import MainLayout from "../layouts/MainLayout";
import DashboardPage from "../features/dashboard/DashboardPage";
import ApprovalQueuePage from "../features/approvals/ApprovalQueuePage";
import AdminPage from "../pages/AdminPage";
import EditRequestPage from "../pages/EditRequestPage";
import LoginPage from "../pages/LoginPage";
import NotFoundPage from "../pages/NotFoundPage";
import ProfilePage from "../pages/ProfilePage";
import RequestDetailPage from "../features/requests/RequestDetailPage";
import RequestsPage from "../features/requests/RequestsPage";
import CreateRequestPage from "../features/requests/CreateRequestPage";

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
        <Route path="requests/new" element={<CreateRequestPage />} />
        <Route path="requests/:id" element={<RequestDetailPage />} />
        <Route path="requests/:id/edit" element={<EditRequestPage />} />
        <Route
          path="approvals"
          element={(
            <ManagerOrAdminRoute>
              <ApprovalQueuePage />
            </ManagerOrAdminRoute>
          )}
        />
        <Route path="admin" element={<AdminPage />} />
        <Route path="profile" element={<ProfilePage />} />
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}

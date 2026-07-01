import { get } from "../../api/request";
import type { DashboardSummary } from "./dashboardTypes";

export function getDashboard() {
  return get<DashboardSummary>("/dashboard");
}

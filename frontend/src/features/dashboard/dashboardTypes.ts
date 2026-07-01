export interface DashboardSummary {
  draftCount: number;
  submittedCount: number;
  pendingApprovalCount: number;
  approvedCount: number;
  rejectedCount: number;
  recentRequests: RecentRequest[];
}

export interface RecentRequest {
  id: string;
  title: string;
  status: string;
  updatedAt: string;
  createdBy: string;
}

export type RequestStatus =
  | "Draft"
  | "Submitted"
  | "UnderReview"
  | "Approved"
  | "Rejected"
  | "Cancelled";

export type RequestCategory =
  | "Equipment"
  | "Training"
  | "SoftwareAccess"
  | "Expense"
  | "Leave"
  | "Other";

export type SortDirection = "asc" | "desc";

export type RequestListItem = {
  id: string;
  title: string;
  description: string;
  category: RequestCategory;
  status: RequestStatus;
  createdByName: string;
  assignedReviewerName: string | null;
  createdAt: string;
  updatedAt: string;
  submittedAt: string | null;
};

export type PaginatedResponse<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type RequestListParams = {
  page: number;
  pageSize: number;
  search?: string;
  status?: RequestStatus;
  category?: RequestCategory;
  sortBy: "updatedAt" | "createdAt" | "title" | "status";
  sortDirection: SortDirection;
};

export interface CreateRequestRequest {
  title: string;
  description: string;
  category: RequestCategory;
  submit: boolean;
}

export interface CreateRequestResponse {
  id: string;
  status: RequestStatus;
}

export type RequestUserSummary = {
  id: string;
  name: string;
  email: string;
};

export type RequestComment = {
  id: string;
  content: string;
  createdAt: string;
  author: RequestUserSummary;
};

export type RequestAuditEntry = {
  id: string;
  action: string;
  description: string | null;
  createdAt: string;
  performedBy: RequestUserSummary;
};

export type RequestDetail = {
  id: string;
  title: string;
  description: string;
  category: RequestCategory;
  status: RequestStatus;
  createdBy: RequestUserSummary;
  assignedReviewer: RequestUserSummary | null;
  createdAt: string;
  updatedAt: string;
  submittedAt: string | null;
  reviewedAt: string | null;
  cancelledAt: string | null;
  rejectionReason: string | null;
  comments: RequestComment[];
  auditLogs: RequestAuditEntry[];
};

export type AddCommentRequest = {
  content: string;
};

export type RejectRequestRequest = {
  reason: string;
};
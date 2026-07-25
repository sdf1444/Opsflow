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
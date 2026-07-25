import type {
  PaginatedResponse,
  RequestCategory,
  RequestListParams,
  RequestStatus,
} from "../requests/requestTypes";

export type ApprovalQueueItem = {
  id: string;
  title: string;
  category: RequestCategory;
  status: RequestStatus;
  createdByName: string;
  submittedAt: string;
  updatedAt: string;
};

export type ApprovalQueueSummary = {
  pending: number;
  overdue: number;
};

export type ApprovalQueueResponse = PaginatedResponse<ApprovalQueueItem> & {
  summary: ApprovalQueueSummary;
};

export type ApprovalQueueParams = RequestListParams;

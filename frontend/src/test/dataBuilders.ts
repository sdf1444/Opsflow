import type { CurrentUserResponse, UserRole } from "../features/auth/authTypes";
import type {
  RequestAuditEntry,
  RequestCategory,
  RequestComment,
  RequestDetail,
  RequestListItem,
  RequestStatus,
} from "../features/requests/requestTypes";
import type { DashboardSummary } from "../features/dashboard/dashboardTypes";

type DeepPartial<T> = {
  [K in keyof T]?: T[K] extends object ? DeepPartial<T[K]> : T[K];
};

const now = new Date("2026-07-25T11:00:00.000Z").toISOString();

function merge<T>(base: T, override?: DeepPartial<T>): T {
  if (!override) {
    return base;
  }

  return {
    ...base,
    ...override,
  } as T;
}

export function buildUser(override?: DeepPartial<CurrentUserResponse>): CurrentUserResponse {
  return merge(
    {
      id: "user-1",
      email: "employee@example.com",
      name: "Employee User",
      role: "Employee" as UserRole,
    },
    override,
  );
}

export function buildRequest(override?: DeepPartial<RequestListItem>): RequestListItem {
  return merge(
    {
      id: "request-1",
      title: "New development laptop",
      description: "Need a better machine for build performance",
      category: "Equipment" as RequestCategory,
      status: "Submitted" as RequestStatus,
      createdByName: "Employee User",
      assignedReviewerName: "Manager User",
      createdAt: now,
      updatedAt: now,
      submittedAt: now,
    },
    override,
  );
}

export function buildComment(override?: DeepPartial<RequestComment>): RequestComment {
  return merge(
    {
      id: "comment-1",
      content: "Looks good to me.",
      createdAt: now,
      author: {
        id: "user-2",
        name: "Manager User",
        email: "manager@example.com",
      },
    },
    override,
  );
}

export function buildAuditEntry(override?: DeepPartial<RequestAuditEntry>): RequestAuditEntry {
  return merge(
    {
      id: "audit-1",
      action: "RequestSubmitted",
      description: "Submitted for review",
      createdAt: now,
      performedBy: {
        id: "user-1",
        name: "Employee User",
        email: "employee@example.com",
      },
    },
    override,
  );
}

export function buildRequestDetail(override?: DeepPartial<RequestDetail>): RequestDetail {
  return merge(
    {
      id: "request-1",
      title: "New development laptop",
      description: "Need a better machine for build performance",
      category: "Equipment" as RequestCategory,
      status: "Submitted" as RequestStatus,
      createdBy: {
        id: "user-1",
        name: "Employee User",
        email: "employee@example.com",
      },
      assignedReviewer: {
        id: "user-2",
        name: "Manager User",
        email: "manager@example.com",
      },
      createdAt: now,
      updatedAt: now,
      submittedAt: now,
      reviewedAt: null,
      cancelledAt: null,
      rejectionReason: null,
      comments: [buildComment()],
      auditLogs: [buildAuditEntry()],
    },
    override,
  );
}

export function buildDashboard(override?: DeepPartial<DashboardSummary>): DashboardSummary {
  return merge(
    {
      draftCount: 2,
      submittedCount: 3,
      pendingApprovalCount: 1,
      approvedCount: 4,
      rejectedCount: 1,
      recentRequests: [
        {
          id: "request-1",
          title: "New development laptop",
          status: "Submitted",
          updatedAt: now,
          createdBy: "Employee User",
        },
      ],
    },
    override,
  );
}

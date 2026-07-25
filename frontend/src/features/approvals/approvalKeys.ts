import type { ApprovalQueueParams } from "./approvalTypes";

export const approvalKeys = {
  all: ["approvals"] as const,

  list: (params: ApprovalQueueParams) => [...approvalKeys.all, params] as const,
};

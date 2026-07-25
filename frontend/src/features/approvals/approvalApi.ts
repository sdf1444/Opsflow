import { get } from "../../api/request";
import { buildRequestQueryString } from "../requests/requestQuery";
import type { ApprovalQueueParams, ApprovalQueueResponse } from "./approvalTypes";

export function getApprovalQueue(params: ApprovalQueueParams): Promise<ApprovalQueueResponse> {
  const queryString = buildRequestQueryString(params);

  return get<ApprovalQueueResponse>(`/approvals?${queryString}`);
}

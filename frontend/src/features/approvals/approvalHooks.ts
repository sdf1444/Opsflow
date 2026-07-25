import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "../../app/queryKeys";
import { rejectRequest, approveRequest } from "../requests/requestApi";
import { requestKeys } from "../requests/requestKeys";
import { getApprovalQueue } from "./approvalApi";
import { approvalKeys } from "./approvalKeys";
import type { ApprovalQueueParams } from "./approvalTypes";

export function useApprovalQueue(params: ApprovalQueueParams) {
  return useQuery({
    queryKey: approvalKeys.list(params),
    queryFn: () => getApprovalQueue(params),
    placeholderData: keepPreviousData,
  });
}

function useApprovalInvalidation() {
  const queryClient = useQueryClient();

  return async function invalidateAfterAction() {
    await queryClient.invalidateQueries({ queryKey: approvalKeys.all });
    await queryClient.invalidateQueries({ queryKey: requestKeys.all });
    await queryClient.invalidateQueries({ queryKey: queryKeys.dashboard });
  };
}

export function useApproveFromQueue() {
  const invalidateAfterAction = useApprovalInvalidation();

  return useMutation({
    mutationFn: (requestId: string) => approveRequest(requestId),
    onSuccess: invalidateAfterAction,
  });
}

export function useRejectFromQueue() {
  const invalidateAfterAction = useApprovalInvalidation();

  return useMutation({
    mutationFn: ({ requestId, reason }: { requestId: string; reason: string }) =>
      rejectRequest(requestId, { reason }),
    onSuccess: invalidateAfterAction,
  });
}

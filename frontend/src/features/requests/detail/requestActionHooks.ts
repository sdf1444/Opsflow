import { useMutation, useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "../../../app/queryKeys";
import {
  addRequestComment,
  approveRequest,
  cancelRequest,
  rejectRequest,
  submitRequest,
} from "../requestApi";
import { requestKeys } from "../requestKeys";
import type { AddCommentRequest, RejectRequestRequest } from "../requestTypes";

function useRequestDetailInvalidation(id: string) {
  const queryClient = useQueryClient();

  return async function invalidateRequestData() {
    await queryClient.invalidateQueries({ queryKey: requestKeys.detail(id) });
    await queryClient.invalidateQueries({ queryKey: requestKeys.comments(id) });
    await queryClient.invalidateQueries({ queryKey: requestKeys.auditLogs(id) });
    await queryClient.invalidateQueries({ queryKey: requestKeys.lists() });
    await queryClient.invalidateQueries({ queryKey: queryKeys.dashboard });
  };
}

export function useSubmitRequest(id: string) {
  const invalidateRequestData = useRequestDetailInvalidation(id);

  return useMutation({
    mutationFn: () => submitRequest(id),
    onSuccess: invalidateRequestData,
  });
}

export function useApproveRequest(id: string) {
  const invalidateRequestData = useRequestDetailInvalidation(id);

  return useMutation({
    mutationFn: () => approveRequest(id),
    onSuccess: invalidateRequestData,
  });
}

export function useRejectRequest(id: string) {
  const invalidateRequestData = useRequestDetailInvalidation(id);

  return useMutation({
    mutationFn: (request: RejectRequestRequest) => rejectRequest(id, request),
    onSuccess: invalidateRequestData,
  });
}

export function useCancelRequest(id: string) {
  const invalidateRequestData = useRequestDetailInvalidation(id);

  return useMutation({
    mutationFn: () => cancelRequest(id),
    onSuccess: invalidateRequestData,
  });
}

export function useAddRequestComment(id: string) {
  const invalidateRequestData = useRequestDetailInvalidation(id);

  return useMutation({
    mutationFn: (request: AddCommentRequest) => addRequestComment(id, request),
    onSuccess: invalidateRequestData,
  });
}

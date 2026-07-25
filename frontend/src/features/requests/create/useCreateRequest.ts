import { useMutation, useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "../../../app/queryKeys";
import { requestKeys } from "../requestKeys";
import { createRequest } from "../requestApi";
import type { CreateRequestRequest } from "../requestTypes";

export function useCreateRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateRequestRequest) => createRequest(request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: requestKeys.all });
      await queryClient.invalidateQueries({ queryKey: queryKeys.dashboard });
    },
  });
}
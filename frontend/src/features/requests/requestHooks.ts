import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { getRequests } from "./requestApi";
import { requestKeys } from "./requestKeys";
import type { RequestListParams } from "./requestTypes";

export function useRequests(params: RequestListParams) {
  return useQuery({
    queryKey: requestKeys.list(params),
    queryFn: () => getRequests(params),
    placeholderData: keepPreviousData,
  });
}
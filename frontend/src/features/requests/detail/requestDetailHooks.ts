import { useQuery } from "@tanstack/react-query";
import { getRequestById } from "../requestApi";
import { requestKeys } from "../requestKeys";

export function useRequestDetail(id: string) {
  return useQuery({
    queryKey: requestKeys.detail(id),
    queryFn: () => getRequestById(id),
    enabled: Boolean(id),
  });
}

import { useQuery } from "@tanstack/react-query";
import { queryKeys } from "../../app/queryKeys";
import { getDashboard } from "./dashboardApi";

export function useDashboard() {
  return useQuery({
    queryKey: queryKeys.dashboard,
    queryFn: getDashboard,
  });
}

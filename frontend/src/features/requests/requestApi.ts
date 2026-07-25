import { get } from "../../api/request";
import type {
  PaginatedResponse,
  RequestListItem,
  RequestListParams,
} from "./requestTypes";

function buildQueryString(params: RequestListParams): string {
  const searchParams = new URLSearchParams();

  searchParams.set("page", params.page.toString());
  searchParams.set("pageSize", params.pageSize.toString());
  searchParams.set("sortBy", params.sortBy);
  searchParams.set("sortDirection", params.sortDirection);

  if (params.search?.trim()) {
    searchParams.set("search", params.search.trim());
  }

  if (params.status) {
    searchParams.set("status", params.status);
  }

  if (params.category) {
    searchParams.set("category", params.category);
  }

  return searchParams.toString();
}

export function getRequests(params: RequestListParams): Promise<PaginatedResponse<RequestListItem>> {
  const queryString = buildQueryString(params);

  return get<PaginatedResponse<RequestListItem>>(`/requests?${queryString}`);
}
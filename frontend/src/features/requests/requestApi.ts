import { get, post } from "../../api/request";
import type {
  AddCommentRequest,
  CreateRequestRequest,
  CreateRequestResponse,
  PaginatedResponse,
  RejectRequestRequest,
  RequestComment,
  RequestDetail,
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

export function createRequest(request: CreateRequestRequest) {
  return post<CreateRequestResponse, CreateRequestRequest>("/requests", request);
}

export function getRequestById(id: string): Promise<RequestDetail> {
  return get<RequestDetail>(`/requests/${id}`);
}

export function submitRequest(id: string): Promise<RequestDetail> {
  return post<RequestDetail>(`/requests/${id}/submit`);
}

export function approveRequest(id: string): Promise<RequestDetail> {
  return post<RequestDetail>(`/requests/${id}/approve`);
}

export function rejectRequest(id: string, request: RejectRequestRequest): Promise<RequestDetail> {
  return post<RequestDetail, RejectRequestRequest>(`/requests/${id}/reject`, request);
}

export function cancelRequest(id: string): Promise<RequestDetail> {
  return post<RequestDetail>(`/requests/${id}/cancel`);
}

export function addRequestComment(id: string, request: AddCommentRequest): Promise<RequestComment> {
  return post<RequestComment, AddCommentRequest>(`/requests/${id}/comments`, request);
}
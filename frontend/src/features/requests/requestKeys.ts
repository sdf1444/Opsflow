import type { RequestListParams } from "./requestTypes";

export const requestKeys = {
  all: ["requests"] as const,

  lists: () => [...requestKeys.all, "list"] as const,

  list: (params: RequestListParams) => [...requestKeys.lists(), params] as const,

  details: () => [...requestKeys.all, "detail"] as const,

  detail: (id: string) => [...requestKeys.details(), id] as const,

  comments: (id: string) => [...requestKeys.detail(id), "comments"] as const,

  auditLogs: (id: string) => [...requestKeys.detail(id), "audit"] as const,
};
export const queryKeys = {
  dashboard: ["dashboard"] as const,
  requests: ["requests"] as const,
  request: (id: string) => ["requests", id] as const,
  comments: (requestId: string) => ["requests", requestId, "comments"] as const,
};

import { QueryClientProvider } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";
import { HttpResponse, http } from "msw";
import { describe, expect, it } from "vitest";
import type { ReactNode } from "react";
import { createTestQueryClient } from "../../../test/render";
import { server } from "../../../test/server";
import { useRequests } from "../requestHooks";
import { useRequestDetail } from "../detail/requestDetailHooks";

function createWrapper() {
  const queryClient = createTestQueryClient();

  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

describe("request hooks", () => {
  it("useRequests returns paged data", async () => {
    const { result } = renderHook(
      () =>
        useRequests({
          page: 1,
          pageSize: 10,
          sortBy: "updatedAt",
          sortDirection: "desc",
        }),
      { wrapper: createWrapper() },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.items.length).toBeGreaterThan(0);
  });

  it("useRequestDetail returns detail data", async () => {
    const { result } = renderHook(() => useRequestDetail("request-1"), { wrapper: createWrapper() });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.id).toBe("request-1");
  });

  it("useRequestDetail returns error on missing request", async () => {
    server.use(http.get("*/requests/request-404", async () => HttpResponse.json({ title: "Not found" }, { status: 404 })));

    const { result } = renderHook(() => useRequestDetail("request-404"), { wrapper: createWrapper() });

    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});

import { renderHook, waitFor } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { HttpResponse, http } from "msw";
import { QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";
import { useDashboard } from "../dashboardHooks";
import { createTestQueryClient } from "../../../test/render";
import { server } from "../../../test/server";

function createWrapper() {
  const queryClient = createTestQueryClient();

  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

describe("useDashboard", () => {
  it("returns dashboard data", async () => {
    const { result } = renderHook(() => useDashboard(), { wrapper: createWrapper() });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.draftCount).toBeTypeOf("number");
  });

  it("returns error on server failure", async () => {
    server.use(http.get("*/dashboard", async () => HttpResponse.json({ title: "Failed" }, { status: 500 })));

    const { result } = renderHook(() => useDashboard(), { wrapper: createWrapper() });

    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});

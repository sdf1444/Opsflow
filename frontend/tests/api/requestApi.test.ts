import { describe, expect, it } from "vitest";
import { getRequests, getRequestById } from "../../src/features/requests/requestApi";

describe("requestApi with MSW", () => {
  it("fetches paged requests", async () => {
    const response = await getRequests({
      page: 1,
      pageSize: 10,
      sortBy: "updatedAt",
      sortDirection: "desc",
    });

    expect(response.items.length).toBeGreaterThan(0);
    expect(response.totalCount).toBeGreaterThan(0);
  });

  it("fetches request detail", async () => {
    const detail = await getRequestById("request-1");

    expect(detail.id).toBe("request-1");
    expect(detail.title.length).toBeGreaterThan(0);
  });
});

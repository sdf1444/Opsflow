import { HttpResponse, delay, http } from "msw";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import DashboardPage from "../DashboardPage";
import { renderAsEmployee } from "../../../test/auth";
import { buildDashboard } from "../../../test/dataBuilders";
import { server } from "../../../test/server";

describe("DashboardPage", () => {
  it("shows skeleton while loading", async () => {
    server.use(
      http.get("*/dashboard", async () => {
        await delay(300);
        return HttpResponse.json(buildDashboard());
      }),
    );

    renderAsEmployee(<DashboardPage />);

    expect(screen.queryByText("Drafts")).not.toBeInTheDocument();

    expect(await screen.findByText("Drafts")).toBeInTheDocument();
  });

  it("shows dashboard cards on success", async () => {
    renderAsEmployee(<DashboardPage />);

    expect(await screen.findByText("Drafts")).toBeInTheDocument();
    expect(screen.getAllByText("Submitted").length).toBeGreaterThan(0);
    expect(screen.getByText("Pending Approval")).toBeInTheDocument();
    expect(screen.getByText("Approved")).toBeInTheDocument();
    expect(screen.getByText("Rejected")).toBeInTheDocument();
  });

  it("shows zero counts when API returns empty data", async () => {
    server.use(
      http.get("*/dashboard", async () =>
        HttpResponse.json(
          buildDashboard({
            draftCount: 0,
            submittedCount: 0,
            pendingApprovalCount: 0,
            approvedCount: 0,
            rejectedCount: 0,
            recentRequests: [],
          }),
        ),
      ),
    );

    renderAsEmployee(<DashboardPage />);

    expect(await screen.findByText("Drafts")).toBeInTheDocument();
    expect(screen.getAllByText("0").length).toBeGreaterThan(0);
  });

  it("shows retry button on failure and retries", async () => {
    let callCount = 0;

    server.use(
      http.get("*/dashboard", async () => {
        callCount += 1;
        if (callCount === 1) {
          return HttpResponse.json({ title: "Error" }, { status: 500 });
        }

        return HttpResponse.json(buildDashboard());
      }),
    );

    const user = userEvent.setup();
    renderAsEmployee(<DashboardPage />);

    expect(await screen.findByText("Unable to load dashboard.")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Retry" }));

    await waitFor(() => {
      expect(screen.getByText("Drafts")).toBeInTheDocument();
    });
  });
});

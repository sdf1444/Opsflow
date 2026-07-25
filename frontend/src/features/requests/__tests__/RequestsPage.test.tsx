import { HttpResponse, http } from "msw";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import RequestsPage from "../RequestsPage";
import { renderAsEmployee } from "../../../test/auth";
import { buildRequest } from "../../../test/dataBuilders";
import { setMockRequests } from "../../../test/handlers";
import { server } from "../../../test/server";

describe("RequestsPage", () => {
  it("supports search and status/category filters", async () => {
    setMockRequests([
      buildRequest({ id: "r1", title: "Laptop purchase", status: "Submitted", category: "Equipment" }),
      buildRequest({ id: "r2", title: "Conference travel", status: "Draft", category: "Leave" }),
    ]);

    const user = userEvent.setup();
    renderAsEmployee(<RequestsPage />);

    expect(await screen.findByText("Laptop purchase")).toBeInTheDocument();

    await user.type(screen.getByLabelText("Search requests"), "travel");
    await waitFor(() => expect(screen.getByText("Conference travel")).toBeInTheDocument());
    await waitFor(() => {
      expect(screen.queryByText("Laptop purchase")).not.toBeInTheDocument();
    });

    await user.click(screen.getByLabelText("Status"));
    await user.click(screen.getByRole("option", { name: "Draft" }));
    expect(screen.getByText("Conference travel")).toBeInTheDocument();

    await user.click(screen.getByLabelText("Category"));
    await user.click(screen.getByRole("option", { name: "Leave" }));
    expect(screen.getByText("Conference travel")).toBeInTheDocument();
  });

  it("supports sorting", async () => {
    setMockRequests([
      buildRequest({ id: "r1", title: "Beta request" }),
      buildRequest({ id: "r2", title: "Alpha request" }),
    ]);

    const user = userEvent.setup();
    renderAsEmployee(<RequestsPage />);

    await screen.findByText("Beta request");

    await user.click(screen.getByText("Title"));

    await waitFor(() => {
      const rows = screen.getAllByRole("row");
      expect(rows[1]).toHaveTextContent("Alpha request");
    });
  });

  it("shows empty state", async () => {
    setMockRequests([]);

    renderAsEmployee(<RequestsPage />);

    expect(await screen.findByText("No requests found")).toBeInTheDocument();
  });

  it("shows error and retry action", async () => {
    server.use(
      http.get("*/requests", async () => HttpResponse.json({ title: "Error" }, { status: 500 })),
    );

    renderAsEmployee(<RequestsPage />);

    expect(await screen.findByText("Unable to load requests.")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Retry" })).toBeInTheDocument();
  });
});

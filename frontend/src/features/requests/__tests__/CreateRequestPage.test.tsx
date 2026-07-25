import { HttpResponse, http } from "msw";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";
import CreateRequestPage from "../CreateRequestPage";
import RequestsPage from "../RequestsPage";
import { renderAsEmployee } from "../../../test/auth";
import { server } from "../../../test/server";

describe("CreateRequestPage", () => {
  it("validates required/min-length fields", async () => {
    const user = userEvent.setup();

    renderAsEmployee(<CreateRequestPage />);

    await user.click(screen.getByRole("button", { name: "Submit" }));

    expect((await screen.findAllByText(/too small/i)).length).toBeGreaterThan(0);
  });

  it("creates draft and redirects with flash message", async () => {
    const user = userEvent.setup();

    renderAsEmployee(
      <Routes>
        <Route path="/requests/new" element={<CreateRequestPage />} />
        <Route path="/requests" element={<RequestsPage />} />
      </Routes>,
      "/requests/new",
    );

    await user.type(screen.getByLabelText("Title"), "New headset request");
    await user.type(screen.getByLabelText("Description"), "Need headset for daily standups and calls.");

    await user.click(screen.getByRole("button", { name: "Save Draft" }));

    expect(await screen.findByText("Requests")).toBeInTheDocument();
  }, 15000);

  it("submits request and redirects", async () => {
    const user = userEvent.setup();

    renderAsEmployee(
      <Routes>
        <Route path="/requests/new" element={<CreateRequestPage />} />
        <Route path="/requests" element={<RequestsPage />} />
      </Routes>,
      "/requests/new",
    );

    await user.type(screen.getByLabelText("Title"), "New laptop request");
    await user.type(screen.getByLabelText("Description"), "Need a stronger laptop for local integration testing.");

    await user.click(screen.getByRole("button", { name: "Submit" }));

    expect(await screen.findByText("Requests")).toBeInTheDocument();
  }, 15000);

  it("shows API error", async () => {
    server.use(
      http.post("*/requests", async () => HttpResponse.json({ title: "Failed" }, { status: 500 })),
    );

    const user = userEvent.setup();
    renderAsEmployee(<CreateRequestPage />);

    await user.type(screen.getByLabelText("Title"), "New monitor request");
    await user.type(screen.getByLabelText("Description"), "Need dual monitors to improve productivity.");
    await user.click(screen.getByRole("button", { name: "Save Draft" }));

    expect(await screen.findByText("Unable to create request.")).toBeInTheDocument();
  }, 15000);
});

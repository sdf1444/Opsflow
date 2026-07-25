import { HttpResponse, http } from "msw";
import { screen } from "@testing-library/react";
import { Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";
import RequestDetailPage from "../RequestDetailPage";
import { renderAsEmployee, renderAsManager, renderAsAdmin } from "../../../test/auth";
import { buildRequestDetail } from "../../../test/dataBuilders";
import { setMockRequestDetail } from "../../../test/handlers";
import { server } from "../../../test/server";

function renderDetailAsEmployee() {
  return renderAsEmployee(
    <Routes>
      <Route path="/requests/:id" element={<RequestDetailPage />} />
    </Routes>,
    "/requests/request-1",
  );
}

describe("RequestDetailPage with MSW", () => {
  it("shows title, status, description, comments, and audit log", async () => {
    setMockRequestDetail(
      "request-1",
      buildRequestDetail({
        title: "Laptop refresh",
        description: "Replacing an underpowered device.",
      }),
    );

    renderDetailAsEmployee();

    expect(await screen.findByText("Laptop refresh")).toBeInTheDocument();
    expect(screen.getByText("Submitted")).toBeInTheDocument();
    expect(screen.getByText("Replacing an underpowered device.")).toBeInTheDocument();
    expect(screen.getByText("Comments")).toBeInTheDocument();
    expect(screen.getByText("Activity")).toBeInTheDocument();
  });

  it("shows submit action for employee draft owner", async () => {
    setMockRequestDetail(
      "request-1",
      buildRequestDetail({
        status: "Draft",
        createdBy: { id: "user-1", name: "Employee User", email: "employee@example.com" },
      }),
    );

    renderDetailAsEmployee();

    expect(await screen.findByRole("button", { name: "Submit" })).toBeInTheDocument();
  });

  it("shows approve action for assigned manager", async () => {
    setMockRequestDetail(
      "request-1",
      buildRequestDetail({
        status: "Submitted",
        assignedReviewer: { id: "manager-1", name: "Manager User", email: "manager@example.com" },
      }),
    );

    renderAsManager(
      <Routes>
        <Route path="/requests/:id" element={<RequestDetailPage />} />
      </Routes>,
      "/requests/request-1",
    );

    expect(await screen.findByRole("button", { name: "Approve" })).toBeInTheDocument();
  });

  it("shows admin capabilities", async () => {
    setMockRequestDetail(
      "request-1",
      buildRequestDetail({
        status: "Submitted",
      }),
    );

    renderAsAdmin(
      <Routes>
        <Route path="/requests/:id" element={<RequestDetailPage />} />
      </Routes>,
      "/requests/request-1",
    );

    expect(await screen.findByRole("button", { name: "Approve" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Reject" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Cancel request" })).toBeInTheDocument();
  });

  it("shows 404 state", async () => {
    server.use(http.get("*/requests/request-404", async () => HttpResponse.json({ title: "Not found" }, { status: 404 })));

    renderAsEmployee(
      <Routes>
        <Route path="/requests/:id" element={<RequestDetailPage />} />
      </Routes>,
      "/requests/request-404",
    );

    expect(await screen.findByText("Request not found")).toBeInTheDocument();
  });
});

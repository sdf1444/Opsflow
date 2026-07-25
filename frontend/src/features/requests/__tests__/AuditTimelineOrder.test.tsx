import { screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { AuditTimeline } from "../components/AuditTimeline";
import { renderWithProviders } from "../../../test/render";

describe("AuditTimeline order and labels", () => {
  it("renders newest first with friendly labels", () => {
    renderWithProviders(
      <AuditTimeline
        entries={[
          {
            id: "older",
            action: "RequestSubmitted",
            description: "Submitted",
            createdAt: "2026-07-24T10:00:00.000Z",
            performedBy: { id: "u1", name: "User A", email: "a@example.com" },
          },
          {
            id: "newer",
            action: "RequestApproved",
            description: "Approved",
            createdAt: "2026-07-25T10:00:00.000Z",
            performedBy: { id: "u2", name: "User B", email: "b@example.com" },
          },
        ]}
      />, 
    );

    const sections = screen.getAllByText(/Request (Approved|Submitted)/);
    expect(sections[0]).toHaveTextContent("Request Approved");
    expect(sections[1]).toHaveTextContent("Request Submitted");

    const activity = screen.getByText("Activity").closest("div");
    if (activity) {
      expect(within(activity).getByText("Request Approved")).toBeInTheDocument();
    }
  });
});

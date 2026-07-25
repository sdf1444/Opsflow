import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { AuditTimeline } from "./AuditTimeline";

describe("AuditTimeline", () => {
  it("uses friendly action labels", () => {
    render(
      <AuditTimeline
        entries={[
          {
            id: "1",
            action: "RequestApproved",
            description: null,
            createdAt: new Date().toISOString(),
            performedBy: {
              id: "u1",
              name: "Manager One",
              email: "manager@example.com",
            },
          },
        ]}
      />, 
    );

    expect(screen.getByText("Request Approved")).toBeInTheDocument();
  });
});

import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { RequestStatusChip } from "./RequestStatusChip";

describe("RequestStatusChip", () => {
  it("renders correct status labels", () => {
    render(
      <>
        <RequestStatusChip status="Draft" />
        <RequestStatusChip status="UnderReview" />
        <RequestStatusChip status="Cancelled" />
      </>,
    );

    expect(screen.getByText("Draft")).toBeInTheDocument();
    expect(screen.getByText("Under review")).toBeInTheDocument();
    expect(screen.getByText("Cancelled")).toBeInTheDocument();
  });
});

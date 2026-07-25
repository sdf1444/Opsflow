import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { RequestStatusChip } from "../components/RequestStatusChip";
import { renderWithProviders } from "../../../test/render";

describe("RequestStatusChip color mapping", () => {
  it("maps approved to success style and rejected to error style", () => {
    const { rerender } = renderWithProviders(<RequestStatusChip status="Approved" />);
    expect(screen.getByText("Approved").closest(".MuiChip-root")?.className).toContain("MuiChip-colorSuccess");

    rerender(<RequestStatusChip status="Rejected" />);
    expect(screen.getByText("Rejected").closest(".MuiChip-root")?.className).toContain("MuiChip-colorError");
  });
});

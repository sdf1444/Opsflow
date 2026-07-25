import { render } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import EmptyState from "../ui/EmptyState";
import DashboardCard from "../../features/dashboard/components/DashboardCard";
import { RequestStatusChip } from "../../features/requests/components/RequestStatusChip";
import { renderWithProviders } from "../../test/render";

describe("stable component snapshots", () => {
  it("RequestStatusChip snapshot", () => {
    const { container } = renderWithProviders(<RequestStatusChip status="Approved" />);
    expect(container.firstChild).toMatchSnapshot();
  });

  it("DashboardCard snapshot", () => {
    const { container } = renderWithProviders(
      <DashboardCard title="Pending" count={3} icon={<span>P</span>} color="orange" />,
    );
    expect(container.firstChild).toMatchSnapshot();
  });

  it("EmptyState snapshot", () => {
    const { container } = render(
      <EmptyState title="Nothing here" description="No data currently available." action={<button>Retry</button>} />,
    );
    expect(container.firstChild).toMatchSnapshot();
  });
});

import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import RequestsPage from "../../features/requests/RequestsPage";
import { renderAsEmployee } from "../../test/auth";

describe("basic accessibility checks", () => {
  it("has accessible buttons, labeled input, and table headings", async () => {
    const user = userEvent.setup();

    renderAsEmployee(<RequestsPage />);

    expect(await screen.findByRole("heading", { name: "Requests" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "New request" })).toBeInTheDocument();
    expect(await screen.findByLabelText("Search requests")).toBeInTheDocument();
    expect(await screen.findByRole("table", { name: "Requests" })).toBeInTheDocument();
    expect(screen.getByText("Title")).toBeInTheDocument();

    await user.tab();
    expect(document.activeElement).toBeTruthy();
  });
});

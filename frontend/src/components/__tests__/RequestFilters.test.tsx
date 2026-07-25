import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { RequestFilters } from "../../features/requests/components/RequestFilters";
import { renderWithProviders } from "../../test/render";

describe("RequestFilters", () => {
  it("calls callbacks for search/status/category and clear", async () => {
    const user = userEvent.setup();
    const onSearchChange = vi.fn();
    const onStatusChange = vi.fn();
    const onCategoryChange = vi.fn();
    const onClear = vi.fn();

    renderWithProviders(
      <RequestFilters
        search="existing"
        status=""
        category=""
        onSearchChange={onSearchChange}
        onStatusChange={onStatusChange}
        onCategoryChange={onCategoryChange}
        onClear={onClear}
      />,
    );

    await user.type(screen.getByLabelText("Search requests"), "laptop");
    expect(onSearchChange).toHaveBeenCalled();

    await user.click(screen.getByLabelText("Status"));
    await user.click(screen.getByRole("option", { name: "Draft" }));
    expect(onStatusChange).toHaveBeenCalledWith("Draft");

    await user.click(screen.getByLabelText("Category"));
    await user.click(screen.getByRole("option", { name: "Equipment" }));
    expect(onCategoryChange).toHaveBeenCalledWith("Equipment");

    await user.click(screen.getByRole("button", { name: "Clear filters" }));
    expect(onClear).toHaveBeenCalled();
  });
});

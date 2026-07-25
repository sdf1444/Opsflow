import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import type { ComponentProps } from "react";
import { describe, expect, it, vi, beforeEach } from "vitest";
import { RequestActions } from "./RequestActions";

const mocks = vi.hoisted(() => ({
  useSubmitRequest: vi.fn(),
  useApproveRequest: vi.fn(),
  useRejectRequest: vi.fn(),
  useCancelRequest: vi.fn(),
}));

vi.mock("../detail/requestActionHooks", () => ({
  useSubmitRequest: mocks.useSubmitRequest,
  useApproveRequest: mocks.useApproveRequest,
  useRejectRequest: mocks.useRejectRequest,
  useCancelRequest: mocks.useCancelRequest,
}));

function idleMutation() {
  return {
    mutateAsync: vi.fn(),
    isPending: false,
    error: null,
  };
}

function renderActions(props?: Partial<ComponentProps<typeof RequestActions>>) {
  const queryClient = new QueryClient();
  const baseRequest = {
    id: "r-1",
    title: "Request title",
    description: "Request description",
    category: "Equipment" as const,
    status: "Draft" as const,
    createdBy: { id: "u-employee", name: "Employee", email: "employee@example.com" },
    assignedReviewer: { id: "u-manager", name: "Manager", email: "manager@example.com" },
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    submittedAt: null,
    reviewedAt: null,
    cancelledAt: null,
    rejectionReason: null,
    comments: [],
    auditLogs: [],
  };

  return render(
    <MemoryRouter>
      <QueryClientProvider client={queryClient}>
        <RequestActions
          request={baseRequest}
          currentUserId="u-employee"
          currentUserRole="Employee"
          {...props}
        />
      </QueryClientProvider>
    </MemoryRouter>,
  );
}

describe("RequestActions", () => {
  beforeEach(() => {
    mocks.useSubmitRequest.mockReturnValue(idleMutation());
    mocks.useApproveRequest.mockReturnValue(idleMutation());
    mocks.useRejectRequest.mockReturnValue(idleMutation());
    mocks.useCancelRequest.mockReturnValue(idleMutation());
  });

  it("shows Submit for owner of Draft", () => {
    renderActions();

    expect(screen.getByRole("button", { name: "Submit" })).toBeInTheDocument();
  });

  it("hides Approve for Employee", () => {
    renderActions();

    expect(screen.queryByRole("button", { name: "Approve" })).not.toBeInTheDocument();
  });

  it("shows Approve for assigned Manager", () => {
    renderActions({
      currentUserId: "u-manager",
      currentUserRole: "Manager",
      request: {
        id: "r-1",
        title: "Request title",
        description: "Request description",
        category: "Equipment",
        status: "Submitted",
        createdBy: { id: "u-employee", name: "Employee", email: "employee@example.com" },
        assignedReviewer: { id: "u-manager", name: "Manager", email: "manager@example.com" },
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        submittedAt: new Date().toISOString(),
        reviewedAt: null,
        cancelledAt: null,
        rejectionReason: null,
        comments: [],
        auditLogs: [],
      },
    });

    expect(screen.getByRole("button", { name: "Approve" })).toBeInTheDocument();
  });

  it("disables Reject when reason is too short", async () => {
    const user = userEvent.setup();

    renderActions({
      currentUserId: "u-manager",
      currentUserRole: "Manager",
      request: {
        id: "r-1",
        title: "Request title",
        description: "Request description",
        category: "Equipment",
        status: "Submitted",
        createdBy: { id: "u-employee", name: "Employee", email: "employee@example.com" },
        assignedReviewer: { id: "u-manager", name: "Manager", email: "manager@example.com" },
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        submittedAt: new Date().toISOString(),
        reviewedAt: null,
        cancelledAt: null,
        rejectionReason: null,
        comments: [],
        auditLogs: [],
      },
    });

    await user.click(screen.getByRole("button", { name: "Reject" }));
    const reasonInput = screen.getByLabelText("Reason");
    await user.type(reasonInput, "abcd");

    const dialog = screen.getByRole("dialog");
    expect(within(dialog).getByRole("button", { name: "Reject" })).toBeDisabled();
  });
});

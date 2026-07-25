import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import RequestDetailPage from "./RequestDetailPage";

const hookMocks = vi.hoisted(() => ({
  useRequestDetail: vi.fn(),
  useAddRequestComment: vi.fn(),
  useSubmitRequest: vi.fn(),
  useApproveRequest: vi.fn(),
  useRejectRequest: vi.fn(),
  useCancelRequest: vi.fn(),
  useAuth: vi.fn(),
}));

vi.mock("./detail/requestDetailHooks", () => ({
  useRequestDetail: hookMocks.useRequestDetail,
}));

vi.mock("./detail/requestActionHooks", () => ({
  useAddRequestComment: hookMocks.useAddRequestComment,
  useSubmitRequest: hookMocks.useSubmitRequest,
  useApproveRequest: hookMocks.useApproveRequest,
  useRejectRequest: hookMocks.useRejectRequest,
  useCancelRequest: hookMocks.useCancelRequest,
}));

vi.mock("../auth/useAuth", () => ({
  useAuth: hookMocks.useAuth,
}));

function renderPage(initialPath = "/requests/r-1") {
  const client = new QueryClient();

  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <QueryClientProvider client={client}>
        <Routes>
          <Route path="/requests/:id" element={<RequestDetailPage />} />
        </Routes>
      </QueryClientProvider>
    </MemoryRouter>,
  );
}

const baseRequest = {
  id: "r-1",
  title: "Laptop Request",
  description: "Need a replacement laptop",
  category: "Equipment" as const,
  status: "Submitted" as const,
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
};

describe("RequestDetailPage", () => {
  beforeEach(() => {
    hookMocks.useAuth.mockReturnValue({
      user: { id: "u-employee", role: "Employee", name: "Employee", email: "employee@example.com" },
    });

    hookMocks.useAddRequestComment.mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    });

    const idleMutation = {
      mutateAsync: vi.fn(),
      isPending: false,
      error: null,
    };

    hookMocks.useSubmitRequest.mockReturnValue(idleMutation);
    hookMocks.useApproveRequest.mockReturnValue(idleMutation);
    hookMocks.useRejectRequest.mockReturnValue(idleMutation);
    hookMocks.useCancelRequest.mockReturnValue(idleMutation);
  });

  it("renders loading skeleton", () => {
    hookMocks.useRequestDetail.mockReturnValue({
      isLoading: true,
      isError: false,
      error: null,
      data: undefined,
      refetch: vi.fn(),
    });

    renderPage();

    expect(screen.getByTestId("request-detail-skeleton")).toBeInTheDocument();
  });

  it("renders request-not-found message for 404", () => {
    hookMocks.useRequestDetail.mockReturnValue({
      isLoading: false,
      isError: true,
      error: { isAxiosError: true, response: { status: 404, data: { title: "Not Found" } } },
      data: undefined,
      refetch: vi.fn(),
    });

    renderPage();

    expect(screen.getByText("Request not found")).toBeInTheDocument();
  });

  it("renders title and status on success", () => {
    hookMocks.useRequestDetail.mockReturnValue({
      isLoading: false,
      isError: false,
      error: null,
      data: baseRequest,
      refetch: vi.fn(),
    });

    renderPage();

    expect(screen.getByText("Laptop Request")).toBeInTheDocument();
    expect(screen.getAllByText("Submitted").length).toBeGreaterThan(0);
  });

  it("shows rejection reason for rejected request", () => {
    hookMocks.useRequestDetail.mockReturnValue({
      isLoading: false,
      isError: false,
      error: null,
      data: {
        ...baseRequest,
        status: "Rejected",
        rejectionReason: "Missing business justification",
      },
      refetch: vi.fn(),
    });

    renderPage();

    expect(screen.getByText("Rejected: Missing business justification")).toBeInTheDocument();
  });
});

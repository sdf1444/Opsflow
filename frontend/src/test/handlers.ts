import { HttpResponse, delay, http } from "msw";
import { buildDashboard, buildRequest, buildRequestDetail, buildUser } from "./dataBuilders";
import type { DashboardSummary } from "../features/dashboard/dashboardTypes";
import type { RequestDetail, RequestListItem, RequestStatus } from "../features/requests/requestTypes";

let mockDashboard: DashboardSummary = buildDashboard();
let mockRequests: RequestListItem[] = [
  buildRequest({ id: "request-1", title: "New development laptop", category: "Equipment", status: "Submitted" }),
  buildRequest({ id: "request-2", title: "Conference training budget", category: "Training", status: "Draft" }),
  buildRequest({ id: "request-3", title: "Expense reimbursement", category: "Expense", status: "Approved" }),
];
let mockRequestDetails: Record<string, RequestDetail> = {
  "request-1": buildRequestDetail({ id: "request-1", title: "New development laptop" }),
  "request-2": buildRequestDetail({ id: "request-2", title: "Conference training budget", status: "Draft", comments: [] }),
  "request-3": buildRequestDetail({ id: "request-3", title: "Expense reimbursement", status: "Approved" }),
};

export function resetMockData() {
  mockDashboard = buildDashboard();
  mockRequests = [
    buildRequest({ id: "request-1", title: "New development laptop", category: "Equipment", status: "Submitted" }),
    buildRequest({ id: "request-2", title: "Conference training budget", category: "Training", status: "Draft" }),
    buildRequest({ id: "request-3", title: "Expense reimbursement", category: "Expense", status: "Approved" }),
  ];
  mockRequestDetails = {
    "request-1": buildRequestDetail({ id: "request-1", title: "New development laptop" }),
    "request-2": buildRequestDetail({ id: "request-2", title: "Conference training budget", status: "Draft", comments: [] }),
    "request-3": buildRequestDetail({ id: "request-3", title: "Expense reimbursement", status: "Approved" }),
  };
}

export function setMockDashboard(value: DashboardSummary) {
  mockDashboard = value;
}

export function setMockRequests(value: RequestListItem[]) {
  mockRequests = value;
}

export function setMockRequestDetail(id: string, value: RequestDetail) {
  mockRequestDetails[id] = value;
}

function applyRequestFilters(items: RequestListItem[], requestUrl: URL): RequestListItem[] {
  const search = requestUrl.searchParams.get("search")?.trim().toLowerCase();
  const status = requestUrl.searchParams.get("status") as RequestStatus | null;
  const category = requestUrl.searchParams.get("category");
  const sortBy = requestUrl.searchParams.get("sortBy") ?? "updatedAt";
  const sortDirection = requestUrl.searchParams.get("sortDirection") ?? "desc";

  let filtered = [...items];

  if (search) {
    filtered = filtered.filter((item) =>
      item.title.toLowerCase().includes(search) || item.description.toLowerCase().includes(search),
    );
  }

  if (status) {
    filtered = filtered.filter((item) => item.status === status);
  }

  if (category) {
    filtered = filtered.filter((item) => item.category === category);
  }

  filtered.sort((left, right) => {
    const leftValue = left[sortBy as keyof RequestListItem];
    const rightValue = right[sortBy as keyof RequestListItem];

    if (leftValue === rightValue) {
      return 0;
    }

    const compare = String(leftValue).localeCompare(String(rightValue));
    return sortDirection === "asc" ? compare : -compare;
  });

  return filtered;
}

export const handlers = [
  http.post("*/auth/login", async () => {
    await delay(50);
    const user = buildUser();

    return HttpResponse.json({
      token: "test-token",
      email: user.email,
      name: user.name,
      role: user.role,
    });
  }),

  http.post("*/login", async () => {
    await delay(50);
    const user = buildUser();

    return HttpResponse.json({
      token: "test-token",
      email: user.email,
      name: user.name,
      role: user.role,
    });
  }),

  http.get("*/dashboard", async () => {
    await delay(50);
    return HttpResponse.json(mockDashboard);
  }),

  http.get("*/requests", async ({ request }) => {
    await delay(50);
    const url = new URL(request.url);
    const page = Number(url.searchParams.get("page") ?? "1");
    const pageSize = Number(url.searchParams.get("pageSize") ?? "10");

    const filtered = applyRequestFilters(mockRequests, url);
    const totalCount = filtered.length;
    const totalPages = totalCount === 0 ? 0 : Math.ceil(totalCount / pageSize);
    const items = filtered.slice((page - 1) * pageSize, page * pageSize);

    return HttpResponse.json({
      items,
      page,
      pageSize,
      totalCount,
      totalPages,
    });
  }),

  http.get("*/requests/:id", async ({ params }) => {
    await delay(50);
    const request = mockRequestDetails[String(params.id)];

    if (!request) {
      return HttpResponse.json({ title: "Not Found", detail: "Request not found" }, { status: 404 });
    }

    return HttpResponse.json(request);
  }),

  http.post("*/requests", async ({ request }) => {
    await delay(50);
    const body = (await request.json()) as {
      title: string;
      description: string;
      category: RequestListItem["category"];
      submit: boolean;
    };

    const id = `request-${mockRequests.length + 1}`;
    const status: RequestStatus = body.submit ? "Submitted" : "Draft";
    const created = buildRequest({
      id,
      title: body.title,
      description: body.description,
      category: body.category,
      status,
      submittedAt: body.submit ? new Date().toISOString() : null,
    });

    mockRequests.unshift(created);
    mockRequestDetails[id] = buildRequestDetail({
      id,
      title: created.title,
      description: created.description,
      category: created.category,
      status: created.status,
      comments: [],
      auditLogs: [],
    });

    return HttpResponse.json({ id, status }, { status: 201 });
  }),

  http.post("*/requests/:id/comments", async ({ params, request }) => {
    await delay(50);
    const requestId = String(params.id);
    const body = (await request.json()) as { content: string };
    const target = mockRequestDetails[requestId];

    if (!target) {
      return HttpResponse.json({ title: "Not Found", detail: "Request not found" }, { status: 404 });
    }

    const comment = {
      id: `comment-${target.comments.length + 1}`,
      content: body.content,
      createdAt: new Date().toISOString(),
      author: {
        id: "user-1",
        name: "Employee User",
        email: "employee@example.com",
      },
    };

    target.comments = [...target.comments, comment];

    return HttpResponse.json(comment, { status: 201 });
  }),

  http.post("*/comments", async ({ request }) => {
    await delay(50);
    const body = (await request.json()) as { content: string };

    return HttpResponse.json(
      {
        id: "comment-standalone-1",
        content: body.content,
        createdAt: new Date().toISOString(),
        author: {
          id: "user-1",
          name: "Employee User",
          email: "employee@example.com",
        },
      },
      { status: 201 },
    );
  }),
];

import { Alert, Box, Button, Paper, Stack, Typography } from "@mui/material";
import { useEffect, useMemo, useState } from "react";
import { RequestFilters } from "../requests/components/RequestFilters";
import type { RequestCategory, RequestStatus } from "../requests/requestTypes";
import { ApprovalQueueStats } from "./components/ApprovalQueueStats";
import { ApprovalQueueTable } from "./components/ApprovalQueueTable";
import { QuickApprovalDialog } from "./components/QuickApprovalDialog";
import { useApprovalQueue, useApproveFromQueue, useRejectFromQueue } from "./approvalHooks";
import type { ApprovalQueueItem, ApprovalQueueParams } from "./approvalTypes";

export default function ApprovalQueuePage() {
  const [searchInput, setSearchInput] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [status, setStatus] = useState<RequestStatus | "">("Submitted");
  const [category, setCategory] = useState<RequestCategory | "">("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [sortBy, setSortBy] = useState<ApprovalQueueParams["sortBy"]>("updatedAt");
  const [sortDirection, setSortDirection] = useState<ApprovalQueueParams["sortDirection"]>("desc");

  const [dialogState, setDialogState] = useState<{
    open: boolean;
    mode: "approve" | "reject";
    request: ApprovalQueueItem | null;
  }>({
    open: false,
    mode: "approve",
    request: null,
  });

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedSearch(searchInput);
      setPage(1);
    }, 350);

    return () => window.clearTimeout(timeoutId);
  }, [searchInput]);

  const params: ApprovalQueueParams = {
    page,
    pageSize,
    search: debouncedSearch || undefined,
    status: status || undefined,
    category: category || undefined,
    sortBy,
    sortDirection,
  };

  const queueQuery = useApprovalQueue(params);
  const approveMutation = useApproveFromQueue();
  const rejectMutation = useRejectFromQueue();

  const actingRequestId = useMemo(() => {
    if (approveMutation.isPending && typeof approveMutation.variables === "string") {
      return approveMutation.variables;
    }

    if (rejectMutation.isPending && rejectMutation.variables) {
      return rejectMutation.variables.requestId;
    }

    return null;
  }, [approveMutation.isPending, approveMutation.variables, rejectMutation.isPending, rejectMutation.variables]);

  function handleStatusChange(value: RequestStatus | "") {
    setStatus(value);
    setPage(1);
  }

  function handleCategoryChange(value: RequestCategory | "") {
    setCategory(value);
    setPage(1);
  }

  function handleSortChange(column: "title" | "status" | "createdAt" | "updatedAt") {
    if (sortBy === column) {
      setSortDirection((current) => (current === "asc" ? "desc" : "asc"));
    } else {
      setSortBy(column);
      setSortDirection("asc");
    }

    setPage(1);
  }

  function clearFilters() {
    setSearchInput("");
    setDebouncedSearch("");
    setStatus("Submitted");
    setCategory("");
    setPage(1);
  }

  function openDialog(mode: "approve" | "reject", request: ApprovalQueueItem) {
    setDialogState({
      open: true,
      mode,
      request,
    });
  }

  async function handleConfirmApprove() {
    if (!dialogState.request) {
      return;
    }

    await approveMutation.mutateAsync(dialogState.request.id);
    setDialogState({ open: false, mode: "approve", request: null });
  }

  async function handleConfirmReject(reason: string) {
    if (!dialogState.request) {
      return;
    }

    await rejectMutation.mutateAsync({ requestId: dialogState.request.id, reason });
    setDialogState({ open: false, mode: "approve", request: null });
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4">Approval Queue</Typography>
        <Typography color="text.secondary">Pending approvals assigned to you.</Typography>
      </Box>

      {queueQuery.data && <ApprovalQueueStats summary={queueQuery.data.summary} />}

      <Paper variant="outlined" sx={{ p: 2 }}>
        <RequestFilters
          search={searchInput}
          status={status}
          category={category}
          onSearchChange={setSearchInput}
          onStatusChange={handleStatusChange}
          onCategoryChange={handleCategoryChange}
          onClear={clearFilters}
        />
      </Paper>

      {queueQuery.isError && (
        <Alert
          severity="error"
          action={
            <Button color="inherit" size="small" onClick={() => queueQuery.refetch()}>
              Retry
            </Button>
          }
        >
          Unable to load approval queue.
        </Alert>
      )}

      {queueQuery.isLoading && (
        <Paper variant="outlined" sx={{ p: 4 }}>
          <Typography color="text.secondary">Loading approvals...</Typography>
        </Paper>
      )}

      {!queueQuery.isLoading && !queueQuery.isError && queueQuery.data && queueQuery.data.items.length === 0 && (
        <Paper variant="outlined" sx={{ p: 6, textAlign: "center" }}>
          <Typography variant="h6">You're all caught up.</Typography>
          <Typography color="text.secondary" sx={{ mt: 1 }}>
            No requests currently require review.
          </Typography>
        </Paper>
      )}

      {!queueQuery.isLoading && !queueQuery.isError && queueQuery.data && queueQuery.data.items.length > 0 && (
        <ApprovalQueueTable
          items={queueQuery.data.items}
          page={queueQuery.data.page}
          pageSize={queueQuery.data.pageSize}
          totalCount={queueQuery.data.totalCount}
          sortBy={sortBy}
          sortDirection={sortDirection}
          isFetching={queueQuery.isFetching}
          actingRequestId={actingRequestId}
          onPageChange={setPage}
          onPageSizeChange={(value) => {
            setPageSize(value);
            setPage(1);
          }}
          onSortChange={handleSortChange}
          onApprove={(request) => openDialog("approve", request)}
          onReject={(request) => openDialog("reject", request)}
        />
      )}

      <QuickApprovalDialog
        open={dialogState.open}
        mode={dialogState.mode}
        requestTitle={dialogState.request?.title ?? ""}
        isSubmitting={approveMutation.isPending || rejectMutation.isPending}
        error={approveMutation.error ?? rejectMutation.error}
        onCancel={() => setDialogState({ open: false, mode: "approve", request: null })}
        onConfirmApprove={handleConfirmApprove}
        onConfirmReject={handleConfirmReject}
      />
    </Stack>
  );
}

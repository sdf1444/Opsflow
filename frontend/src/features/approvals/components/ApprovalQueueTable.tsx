import DoneIcon from "@mui/icons-material/Done";
import GppBadIcon from "@mui/icons-material/GppBad";
import { IconButton, Tooltip } from "@mui/material";
import { RequestsTable } from "../../requests/components/RequestsTable";
import type { RequestListItem, RequestListParams } from "../../requests/requestTypes";
import type { ApprovalQueueItem } from "../approvalTypes";

type ApprovalQueueTableProps = {
  items: ApprovalQueueItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  sortBy: RequestListParams["sortBy"];
  sortDirection: RequestListParams["sortDirection"];
  isFetching: boolean;
  actingRequestId: string | null;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  onSortChange: (sortBy: RequestListParams["sortBy"]) => void;
  onApprove: (request: ApprovalQueueItem) => void;
  onReject: (request: ApprovalQueueItem) => void;
};

export function ApprovalQueueTable({
  items,
  page,
  pageSize,
  totalCount,
  sortBy,
  sortDirection,
  isFetching,
  actingRequestId,
  onPageChange,
  onPageSizeChange,
  onSortChange,
  onApprove,
  onReject,
}: ApprovalQueueTableProps) {
  const tableItems: RequestListItem[] = items.map((item) => ({
    id: item.id,
    title: item.title,
    description: "",
    category: item.category,
    status: item.status,
    createdByName: item.createdByName,
    assignedReviewerName: null,
    createdAt: item.submittedAt,
    updatedAt: item.updatedAt,
    submittedAt: item.submittedAt,
  }));

  function renderRowActions(row: RequestListItem) {
    const request = items.find((item) => item.id === row.id);
    if (!request) {
      return null;
    }

    const isBusy = actingRequestId === request.id;

    return (
      <>
        <Tooltip title="Approve">
          <span>
            <IconButton
              size="small"
              aria-label={`Approve ${request.title}`}
              onClick={() => onApprove(request)}
              disabled={isBusy}
            >
              <DoneIcon fontSize="small" />
            </IconButton>
          </span>
        </Tooltip>

        <Tooltip title="Reject">
          <span>
            <IconButton
              size="small"
              aria-label={`Reject ${request.title}`}
              onClick={() => onReject(request)}
              disabled={isBusy}
            >
              <GppBadIcon fontSize="small" />
            </IconButton>
          </span>
        </Tooltip>
      </>
    );
  }

  return (
    <RequestsTable
      requests={tableItems}
      page={page}
      pageSize={pageSize}
      totalCount={totalCount}
      sortBy={sortBy}
      sortDirection={sortDirection}
      isFetching={isFetching}
      onPageChange={onPageChange}
      onPageSizeChange={onPageSizeChange}
      onSortChange={onSortChange}
      showReviewerColumn={false}
      showDescription={false}
      renderActions={renderRowActions}
    />
  );
}

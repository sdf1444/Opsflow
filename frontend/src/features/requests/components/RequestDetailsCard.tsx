import { Alert, Chip, Divider, Paper, Stack, Typography } from "@mui/material";
import { RequestStatusChip } from "./RequestStatusChip";
import type { RequestDetail, RequestStatus } from "../requestTypes";

type RequestDetailsCardProps = {
  request: RequestDetail;
};

function formatDate(value: string | null): string {
  if (!value) {
    return "-";
  }

  return new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function categoryLabel(value: string): string {
  if (value === "SoftwareAccess") {
    return "Software access";
  }

  return value;
}

const statusLabelMap: Record<RequestStatus, string> = {
  Draft: "Draft",
  Submitted: "Submitted",
  UnderReview: "Under review",
  Approved: "Approved",
  Rejected: "Rejected",
  Cancelled: "Cancelled",
};

export function RequestDetailsCard({ request }: RequestDetailsCardProps) {
  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}>
      <Stack spacing={2}>
        <Stack spacing={1}>
          <Typography variant="h4">{request.title}</Typography>

          <Stack direction="row" spacing={1} alignItems="center" useFlexGap flexWrap="wrap">
            <RequestStatusChip status={request.status} />
            <Chip label={categoryLabel(request.category)} size="small" variant="outlined" />
            <Typography variant="body2" color="text.secondary">
              ID: {request.id}
            </Typography>
          </Stack>
        </Stack>

        <Typography color="text.secondary">{request.description}</Typography>

        {request.status === "Rejected" && request.rejectionReason && (
          <Alert severity="warning">Rejected: {request.rejectionReason}</Alert>
        )}

        <Divider />

        <Stack spacing={1}>
          <Typography variant="body2">Created by: {request.createdBy.name}</Typography>
          <Typography variant="body2">Requester email: {request.createdBy.email}</Typography>
          <Typography variant="body2">Assigned reviewer: {request.assignedReviewer?.name ?? "Not assigned"}</Typography>
          <Typography variant="body2" color="text.secondary">
            Created: {formatDate(request.createdAt)}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Updated: {formatDate(request.updatedAt)}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Submitted: {formatDate(request.submittedAt)}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Reviewed: {formatDate(request.reviewedAt)}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Cancelled: {formatDate(request.cancelledAt)}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Status label: {statusLabelMap[request.status]}
          </Typography>
        </Stack>
      </Stack>
    </Paper>
  );
}

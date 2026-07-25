import { Chip } from "@mui/material";
import type { ChipProps } from "@mui/material";
import type { RequestStatus } from "../requestTypes";

type RequestStatusChipProps = {
  status: RequestStatus;
};

const statusColours: Record<RequestStatus, ChipProps["color"]> = {
  Draft: "default",
  Submitted: "info",
  UnderReview: "warning",
  Approved: "success",
  Rejected: "error",
  Cancelled: "default",
};

const statusLabels: Record<RequestStatus, string> = {
  Draft: "Draft",
  Submitted: "Submitted",
  UnderReview: "Under review",
  Approved: "Approved",
  Rejected: "Rejected",
  Cancelled: "Cancelled",
};

export function RequestStatusChip({ status }: RequestStatusChipProps) {
  return (
    <Chip
      label={statusLabels[status]}
      color={statusColours[status]}
      size="small"
      variant={status === "Draft" || status === "Cancelled" ? "outlined" : "filled"}
    />
  );
}
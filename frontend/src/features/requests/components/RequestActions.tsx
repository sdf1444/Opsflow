import EditIcon from "@mui/icons-material/Edit";
import TaskAltIcon from "@mui/icons-material/TaskAlt";
import ThumbDownIcon from "@mui/icons-material/ThumbDown";
import ThumbUpIcon from "@mui/icons-material/ThumbUp";
import { Alert, Button, Stack, TextField, Typography } from "@mui/material";
import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import ConfirmationDialog from "../../../components/ui/ConfirmationDialog";
import { toApiError } from "../../../api/apiError";
import {
  useApproveRequest,
  useCancelRequest,
  useRejectRequest,
  useSubmitRequest,
} from "../detail/requestActionHooks";
import type { RequestDetail } from "../requestTypes";
import type { UserRole } from "../../auth/authTypes";

type RequestActionsProps = {
  request: RequestDetail;
  currentUserId: string;
  currentUserRole: UserRole;
};

export function RequestActions({ request, currentUserId, currentUserRole }: RequestActionsProps) {
  const navigate = useNavigate();

  const submitMutation = useSubmitRequest(request.id);
  const approveMutation = useApproveRequest(request.id);
  const rejectMutation = useRejectRequest(request.id);
  const cancelMutation = useCancelRequest(request.id);

  const [showCancelDialog, setShowCancelDialog] = useState(false);
  const [showRejectDialog, setShowRejectDialog] = useState(false);
  const [rejectionReason, setRejectionReason] = useState("");

  const isOwner = request.createdBy.id === currentUserId;
  const isAdmin = currentUserRole === "Admin";
  const isManager = currentUserRole === "Manager";
  const isAssignedManager = isManager && request.assignedReviewer?.id === currentUserId;

  const canSubmit = isOwner && request.status === "Draft";
  const canEdit = isOwner && request.status === "Draft";
  const canCancel =
    (isOwner || isAdmin) &&
    request.status !== "Approved" &&
    request.status !== "Rejected" &&
    request.status !== "Cancelled";
  const canReview = isAdmin || isAssignedManager;
  const canApprove = canReview && (request.status === "Submitted" || request.status === "UnderReview");
  const canReject = canReview && (request.status === "Submitted" || request.status === "UnderReview");

  const rejectValidationMessage = useMemo(() => {
    const value = rejectionReason.trim();
    if (value.length === 0) {
      return "Reason is required.";
    }

    if (value.length < 5) {
      return "Reason must be at least 5 characters.";
    }

    if (value.length > 1000) {
      return "Reason cannot exceed 1000 characters.";
    }

    return null;
  }, [rejectionReason]);

  const error = submitMutation.error ?? approveMutation.error ?? rejectMutation.error ?? cancelMutation.error;

  async function handleSubmit() {
    await submitMutation.mutateAsync();
  }

  async function handleApprove() {
    await approveMutation.mutateAsync();
  }

  async function handleReject() {
    const reason = rejectionReason.trim();
    if (reason.length < 5 || reason.length > 1000) {
      return;
    }

    await rejectMutation.mutateAsync({ reason });
    setShowRejectDialog(false);
    setRejectionReason("");
  }

  async function handleCancel() {
    await cancelMutation.mutateAsync();
    setShowCancelDialog(false);
  }

  const isBusy =
    submitMutation.isPending || approveMutation.isPending || rejectMutation.isPending || cancelMutation.isPending;

  return (
    <Stack spacing={1.5}>
      <Typography variant="h6">Actions</Typography>

      {error && <Alert severity="error">{toApiError(error).message}</Alert>}

      <Stack direction="row" spacing={1.25} useFlexGap flexWrap="wrap">
        {canEdit && (
          <Button startIcon={<EditIcon />} variant="outlined" onClick={() => navigate(`/requests/${request.id}/edit`)}>
            Edit draft
          </Button>
        )}

        {canSubmit && (
          <Button startIcon={<TaskAltIcon />} variant="contained" onClick={handleSubmit} disabled={isBusy}>
            Submit
          </Button>
        )}

        {canApprove && (
          <Button startIcon={<ThumbUpIcon />} color="success" variant="contained" onClick={handleApprove} disabled={isBusy}>
            Approve
          </Button>
        )}

        {canReject && (
          <Button startIcon={<ThumbDownIcon />} color="error" variant="outlined" onClick={() => setShowRejectDialog(true)}>
            Reject
          </Button>
        )}

        {canCancel && (
          <Button color="inherit" variant="outlined" onClick={() => setShowCancelDialog(true)}>
            Cancel request
          </Button>
        )}
      </Stack>

      <ConfirmationDialog
        open={showCancelDialog}
        title="Cancel request"
        description="This will mark the request as cancelled and close the workflow."
        confirmText="Cancel request"
        confirmColor="warning"
        isConfirming={cancelMutation.isPending}
        onCancel={() => setShowCancelDialog(false)}
        onConfirm={handleCancel}
      />

      <ConfirmationDialog
        open={showRejectDialog}
        title="Reject request"
        description="Provide a reason so the requester understands what needs to change."
        confirmText="Reject"
        confirmColor="error"
        isConfirming={rejectMutation.isPending}
        confirmDisabled={Boolean(rejectValidationMessage)}
        onCancel={() => setShowRejectDialog(false)}
        onConfirm={handleReject}
        content={
          <Stack sx={{ pt: 1 }}>
            <TextField
              label="Reason"
              multiline
              minRows={3}
              value={rejectionReason}
              onChange={(event) => setRejectionReason(event.target.value)}
              fullWidth
              error={Boolean(rejectValidationMessage)}
              helperText={rejectValidationMessage ?? `${rejectionReason.trim().length}/1000`}
            />
          </Stack>
        }
      />
    </Stack>
  );
}

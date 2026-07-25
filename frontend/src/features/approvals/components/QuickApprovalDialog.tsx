import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField } from "@mui/material";
import { useMemo, useState } from "react";
import { toApiError } from "../../../api/apiError";

type QuickApprovalDialogProps = {
  open: boolean;
  mode: "approve" | "reject";
  requestTitle: string;
  isSubmitting: boolean;
  error: unknown;
  onCancel: () => void;
  onConfirmApprove: () => Promise<void>;
  onConfirmReject: (reason: string) => Promise<void>;
};

export function QuickApprovalDialog({
  open,
  mode,
  requestTitle,
  isSubmitting,
  error,
  onCancel,
  onConfirmApprove,
  onConfirmReject,
}: QuickApprovalDialogProps) {
  const [reason, setReason] = useState("");

  const validationMessage = useMemo(() => {
    const trimmed = reason.trim();

    if (mode !== "reject") {
      return null;
    }

    if (trimmed.length === 0) {
      return "Reason is required.";
    }

    if (trimmed.length < 5) {
      return "Reason must be at least 5 characters.";
    }

    if (trimmed.length > 1000) {
      return "Reason cannot exceed 1000 characters.";
    }

    return null;
  }, [mode, reason]);

  async function handleConfirm() {
    if (mode === "approve") {
      await onConfirmApprove();
      return;
    }

    const trimmed = reason.trim();
    if (validationMessage) {
      return;
    }

    await onConfirmReject(trimmed);
    setReason("");
  }

  function handleClose() {
    setReason("");
    onCancel();
  }

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>{mode === "approve" ? "Approve request" : "Reject request"}</DialogTitle>

      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <Alert severity="info">{requestTitle}</Alert>

          {mode === "reject" && (
            <TextField
              label="Reason"
              value={reason}
              onChange={(event) => setReason(event.target.value)}
              multiline
              minRows={3}
              fullWidth
              error={Boolean(validationMessage)}
              helperText={validationMessage ?? `${reason.trim().length}/1000`}
            />
          )}

          {Boolean(error) && <Alert severity="error">{toApiError(error).message}</Alert>}
        </Stack>
      </DialogContent>

      <DialogActions>
        <Button onClick={handleClose} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button
          variant="contained"
          color={mode === "approve" ? "success" : "error"}
          onClick={handleConfirm}
          disabled={isSubmitting || (mode === "reject" && Boolean(validationMessage))}
        >
          {mode === "approve" ? "Approve" : "Reject"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

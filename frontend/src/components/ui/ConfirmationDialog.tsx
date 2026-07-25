import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
} from "@mui/material";
import type { ReactNode } from "react";

type Props = {
  open: boolean;
  title: string;
  description?: string;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: "primary" | "error" | "warning";
  confirmDisabled?: boolean;
  isConfirming?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
  content?: ReactNode;
};

export default function ConfirmationDialog({
  open,
  title,
  description,
  confirmText = "Confirm",
  cancelText = "Cancel",
  confirmColor = "primary",
  confirmDisabled = false,
  isConfirming = false,
  onConfirm,
  onCancel,
  content,
}: Props) {
  return (
    <Dialog open={open} onClose={onCancel} maxWidth="xs" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        {description ? <DialogContentText>{description}</DialogContentText> : null}
        {content}
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel} disabled={isConfirming}>
          {cancelText}
        </Button>
        <Button onClick={onConfirm} color={confirmColor} variant="contained" disabled={isConfirming || confirmDisabled}>
          {confirmText}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

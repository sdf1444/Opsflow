import { Box, Button } from "@mui/material";

type RequestFormActionsProps = {
  onCancel: () => void;
  onSaveDraft: () => void;
  onSubmit: () => void;
  isSaving: boolean;
  savingMode: "draft" | "submit" | null;
};

export function RequestFormActions({
  onCancel,
  onSaveDraft,
  onSubmit,
  isSaving,
  savingMode,
}: RequestFormActionsProps) {
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        gap: 2,
        flexWrap: "wrap",
      }}
    >
      <Button variant="text" onClick={onCancel} disabled={isSaving}>
        Cancel
      </Button>

      <Box sx={{ display: "flex", gap: 1.5, flexWrap: "wrap" }}>
        <Button variant="outlined" onClick={onSaveDraft} disabled={isSaving}>
          {isSaving && savingMode === "draft" ? "Saving..." : "Save Draft"}
        </Button>

        <Button variant="contained" onClick={onSubmit} disabled={isSaving}>
          {isSaving && savingMode === "submit" ? "Submitting..." : "Submit"}
        </Button>
      </Box>
    </Box>
  );
}
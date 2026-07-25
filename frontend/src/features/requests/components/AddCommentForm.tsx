import SendIcon from "@mui/icons-material/Send";
import { Button, Paper, Stack, TextField, Typography } from "@mui/material";
import { useMemo, useState } from "react";

type AddCommentFormProps = {
  isSubmitting: boolean;
  onSubmit: (content: string) => void;
};

export function AddCommentForm({ isSubmitting, onSubmit }: AddCommentFormProps) {
  const [content, setContent] = useState("");
  const [touched, setTouched] = useState(false);

  const trimmed = content.trim();

  const validationMessage = useMemo(() => {
    if (!touched) {
      return null;
    }

    if (trimmed.length < 2) {
      return "Comment must be at least 2 characters.";
    }

    if (trimmed.length > 1000) {
      return "Comment cannot exceed 1000 characters.";
    }

    return null;
  }, [touched, trimmed]);

  const canSubmit = trimmed.length >= 2 && trimmed.length <= 1000 && !isSubmitting;

  function handleSubmit() {
    setTouched(true);
    if (!canSubmit) {
      return;
    }

    onSubmit(trimmed);
    setContent("");
    setTouched(false);
  }

  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}>
      <Stack spacing={1.5}>
        <Typography variant="h6">Add Comment</Typography>

        <TextField
          label="Comment"
          value={content}
          onChange={(event) => setContent(event.target.value)}
          onBlur={() => setTouched(true)}
          multiline
          minRows={3}
          fullWidth
          error={Boolean(validationMessage)}
          helperText={validationMessage ?? `${trimmed.length}/1000`}
        />

        <Stack direction="row" justifyContent="flex-end">
          <Button variant="contained" startIcon={<SendIcon />} disabled={!canSubmit} onClick={handleSubmit}>
            {isSubmitting ? "Posting..." : "Post comment"}
          </Button>
        </Stack>
      </Stack>
    </Paper>
  );
}

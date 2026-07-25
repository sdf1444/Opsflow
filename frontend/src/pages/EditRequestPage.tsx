import { Paper, Stack, Typography } from "@mui/material";

export default function EditRequestPage() {
  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}>
      <Stack spacing={1}>
        <Typography variant="h4">Edit Request</Typography>
        <Typography color="text.secondary">Editing workflow is coming in a follow-up step.</Typography>
      </Stack>
    </Paper>
  );
}

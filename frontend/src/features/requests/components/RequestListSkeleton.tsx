import { Paper, Skeleton, Stack } from "@mui/material";

export function RequestListSkeleton() {
  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={2}>
        {Array.from({ length: 8 }).map((_, index) => (
          <Skeleton key={index} variant="rounded" height={48} />
        ))}
      </Stack>
    </Paper>
  );
}
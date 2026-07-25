import { Paper, Skeleton, Stack } from "@mui/material";

export function RequestDetailSkeleton() {
  return (
    <Stack spacing={2} data-testid="request-detail-skeleton">
      <Paper variant="outlined" sx={{ p: 3 }}>
        <Stack spacing={1.5}>
          <Skeleton variant="text" width="35%" height={44} />
          <Skeleton variant="text" width="60%" />
          <Skeleton variant="rounded" height={120} />
          <Skeleton variant="text" width="80%" />
        </Stack>
      </Paper>

      <Paper variant="outlined" sx={{ p: 3 }}>
        <Stack spacing={1.5}>
          <Skeleton variant="text" width="20%" />
          <Skeleton variant="rounded" height={80} />
          <Skeleton variant="rounded" height={80} />
        </Stack>
      </Paper>

      <Paper variant="outlined" sx={{ p: 3 }}>
        <Stack spacing={1.5}>
          <Skeleton variant="text" width="25%" />
          <Skeleton variant="rounded" height={80} />
          <Skeleton variant="rounded" height={80} />
        </Stack>
      </Paper>
    </Stack>
  );
}

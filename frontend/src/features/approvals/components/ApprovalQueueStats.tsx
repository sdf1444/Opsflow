import PendingActionsIcon from "@mui/icons-material/PendingActions";
import ScheduleIcon from "@mui/icons-material/Schedule";
import { Grid } from "@mui/material";
import DashboardCard from "../../dashboard/components/DashboardCard";
import type { ApprovalQueueSummary } from "../approvalTypes";

type ApprovalQueueStatsProps = {
  summary: ApprovalQueueSummary;
};

export function ApprovalQueueStats({ summary }: ApprovalQueueStatsProps) {
  return (
    <Grid container spacing={2}>
      <Grid size={{ xs: 12, sm: 6 }}>
        <DashboardCard
          title="Pending"
          count={summary.pending}
          icon={<PendingActionsIcon fontSize="small" />}
          color="var(--mui-palette-warning-main)"
        />
      </Grid>

      <Grid size={{ xs: 12, sm: 6 }}>
        <DashboardCard
          title="Overdue"
          count={summary.overdue}
          icon={<ScheduleIcon fontSize="small" />}
          color="var(--mui-palette-error-main)"
        />
      </Grid>
    </Grid>
  );
}

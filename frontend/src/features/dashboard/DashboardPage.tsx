import AssignmentIcon from "@mui/icons-material/Assignment";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import EditNoteIcon from "@mui/icons-material/EditNote";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import PendingActionsIcon from "@mui/icons-material/PendingActions";
import {
  Alert,
  Box,
  Grid,
  Skeleton,
  Stack,
  Typography,
} from "@mui/material";
import { Button, Card, PageHeader } from "../../components/ui";
import { useAuth } from "../auth/useAuth";
import { useDashboard } from "./dashboardHooks";
import DashboardCard from "./components/DashboardCard";
import RecentRequestsTable from "./components/RecentRequestsTable";

function DashboardSkeleton() {
  return (
    <Stack spacing={3}>
      <Box>
        <Skeleton variant="text" width={180} height={48} />
        <Skeleton variant="text" width={260} height={28} />
      </Box>

      <Grid container spacing={3}>
        {Array.from({ length: 5 }).map((_, index) => (
          <Grid key={index} size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
            <Skeleton variant="rounded" height={128} />
          </Grid>
        ))}
      </Grid>

      <Card>
        <Stack spacing={1.5}>
          <Skeleton variant="text" width={220} height={34} />
          {Array.from({ length: 5 }).map((_, index) => (
            <Skeleton key={index} variant="rounded" height={36} />
          ))}
        </Stack>
      </Card>
    </Stack>
  );
}

export default function DashboardPage() {
  const { user } = useAuth();
  const { data, isLoading, error, refetch, isFetching } = useDashboard();

  if (isLoading) {
    return <DashboardSkeleton />;
  }

  if (error || !data) {
    return (
      <Stack spacing={3}>
        <PageHeader title="Dashboard" subtitle={`Welcome back, ${user?.name ?? "there"}`} />
        <Alert
          severity="error"
          action={(
            <Button color="error" variant="outlined" size="small" onClick={() => refetch()}>
              Retry
            </Button>
          )}
        >
          Unable to load dashboard.
        </Alert>
      </Stack>
    );
  }

  const cards = [
    {
      title: "Drafts",
      count: data.draftCount,
      icon: <EditNoteIcon fontSize="small" />,
      color: "text.secondary",
    },
    {
      title: "Submitted",
      count: data.submittedCount,
      icon: <AssignmentIcon fontSize="small" />,
      color: "info.main",
    },
    {
      title: "Pending Approval",
      count: data.pendingApprovalCount,
      icon: <PendingActionsIcon fontSize="small" />,
      color: "warning.main",
    },
    {
      title: "Approved",
      count: data.approvedCount,
      icon: <CheckCircleIcon fontSize="small" />,
      color: "success.main",
    },
    {
      title: "Rejected",
      count: data.rejectedCount,
      icon: <ErrorOutlineIcon fontSize="small" />,
      color: "error.main",
    },
  ];

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Dashboard"
        subtitle={`Welcome back, ${user?.name ?? "there"}`}
        actions={
          isFetching ? (
            <Typography variant="caption" color="text.secondary">
              Refreshing...
            </Typography>
          ) : null
        }
      />

      <Grid container spacing={3}>
        {cards.map((card) => (
          <Grid key={card.title} size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
            <DashboardCard title={card.title} count={card.count} icon={card.icon} color={card.color} />
          </Grid>
        ))}
      </Grid>

      <Card>
        <Stack spacing={2}>
          <Typography variant="h6">Recent Requests</Typography>
          <RecentRequestsTable requests={data.recentRequests} />
        </Stack>
      </Card>
    </Stack>
  );
}

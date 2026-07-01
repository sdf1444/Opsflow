import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { EmptyState, StatusBadge } from "../../../components/ui";
import type { RecentRequest } from "../dashboardTypes";

type Props = {
  requests: RecentRequest[];
};

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "-";
  }

  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(date);
}

export default function RecentRequestsTable({ requests }: Props) {
  if (requests.length === 0) {
    return (
      <EmptyState
        title="No requests yet."
        description="Create your first request."
      />
    );
  }

  return (
    <TableContainer component={Paper} sx={{ overflowX: "auto" }}>
      <Table size="small" sx={{ minWidth: 640 }}>
        <TableHead>
          <TableRow>
            <TableCell>Title</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Updated</TableCell>
            <TableCell>Created By</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {requests.slice(0, 10).map((request) => (
            <TableRow key={request.id} hover>
              <TableCell>
                <Typography variant="body2" sx={{ fontWeight: 500 }}>
                  {request.title}
                </Typography>
              </TableCell>
              <TableCell>
                <StatusBadge status={request.status} />
              </TableCell>
              <TableCell>{formatDate(request.updatedAt)}</TableCell>
              <TableCell>{request.createdBy}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

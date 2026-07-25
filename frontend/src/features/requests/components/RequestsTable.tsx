import OpenInNewIcon from "@mui/icons-material/OpenInNew";
import {
  Box,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  Tooltip,
  Typography,
} from "@mui/material";
import { useNavigate } from "react-router-dom";
import { RequestStatusChip } from "./RequestStatusChip";
import type { RequestListItem, SortDirection } from "../requestTypes";

type SortableColumn = "title" | "status" | "createdAt" | "updatedAt";

type RequestsTableProps = {
  requests: RequestListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  sortBy: SortableColumn;
  sortDirection: SortDirection;
  isFetching: boolean;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  onSortChange: (sortBy: SortableColumn) => void;
};

function formatDate(value: string): string {
  return new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatCategory(category: string): string {
  if (category === "SoftwareAccess") {
    return "Software access";
  }

  return category;
}

export function RequestsTable({
  requests,
  page,
  pageSize,
  totalCount,
  sortBy,
  sortDirection,
  isFetching,
  onPageChange,
  onPageSizeChange,
  onSortChange,
}: RequestsTableProps) {
  const navigate = useNavigate();

  function renderSortLabel(column: SortableColumn, label: string): string {
    if (sortBy !== column) {
      return label;
    }

    return `${label} ${sortDirection === "asc" ? "↑" : "↓"}`;
  }

  return (
    <Paper variant="outlined">
      <TableContainer>
        <Table
          aria-label="Requests"
          sx={{
            opacity: isFetching ? 0.65 : 1,
            transition: "opacity 150ms",
          }}
        >
          <TableHead>
            <TableRow>
              <TableCell onClick={() => onSortChange("title")} sx={{ cursor: "pointer", fontWeight: 600 }}>
                {renderSortLabel("title", "Title")}
              </TableCell>

              <TableCell>Category</TableCell>

              <TableCell onClick={() => onSortChange("status")} sx={{ cursor: "pointer", fontWeight: 600 }}>
                {renderSortLabel("status", "Status")}
              </TableCell>

              <TableCell>Requester</TableCell>

              <TableCell>Reviewer</TableCell>

              <TableCell onClick={() => onSortChange("updatedAt")} sx={{ cursor: "pointer", fontWeight: 600 }}>
                {renderSortLabel("updatedAt", "Last updated")}
              </TableCell>

              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>

          <TableBody>
            {requests.map((request) => (
              <TableRow key={request.id} hover tabIndex={0} onDoubleClick={() => navigate(`/requests/${request.id}`)}>
                <TableCell>
                  <Typography variant="body2" fontWeight={600}>
                    {request.title}
                  </Typography>

                  <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{
                      display: "block",
                      maxWidth: 320,
                      overflow: "hidden",
                      textOverflow: "ellipsis",
                      whiteSpace: "nowrap",
                    }}
                  >
                    {request.description}
                  </Typography>
                </TableCell>

                <TableCell>{formatCategory(request.category)}</TableCell>

                <TableCell>
                  <RequestStatusChip status={request.status} />
                </TableCell>

                <TableCell>{request.createdByName}</TableCell>

                <TableCell>{request.assignedReviewerName ?? "Not assigned"}</TableCell>

                <TableCell>{formatDate(request.updatedAt)}</TableCell>

                <TableCell align="right">
                  <Tooltip title="Open request">
                    <IconButton aria-label={`Open ${request.title}`} onClick={() => navigate(`/requests/${request.id}`)}>
                      <OpenInNewIcon />
                    </IconButton>
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Box sx={{ borderTop: 1, borderColor: "divider" }}>
        <TablePagination
          component="div"
          count={totalCount}
          page={page - 1}
          rowsPerPage={pageSize}
          rowsPerPageOptions={[10, 20, 50]}
          onPageChange={(_, nextPage) => onPageChange(nextPage + 1)}
          onRowsPerPageChange={(event) => onPageSizeChange(Number(event.target.value))}
        />
      </Box>
    </Paper>
  );
}
import AddIcon from "@mui/icons-material/Add";
import { Alert, Box, Button, Paper, Stack, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { RequestFilters } from "./components/RequestFilters";
import { RequestListSkeleton } from "./components/RequestListSkeleton";
import { RequestsTable } from "./components/RequestsTable";
import { useRequests } from "./requestHooks";
import type { RequestCategory, RequestListParams, RequestStatus } from "./requestTypes";

export default function RequestsPage() {
  const navigate = useNavigate();

  const [searchInput, setSearchInput] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [status, setStatus] = useState<RequestStatus | "">("");
  const [category, setCategory] = useState<RequestCategory | "">("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [sortBy, setSortBy] = useState<RequestListParams["sortBy"]>("updatedAt");
  const [sortDirection, setSortDirection] = useState<RequestListParams["sortDirection"]>("desc");

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedSearch(searchInput);
      setPage(1);
    }, 350);

    return () => window.clearTimeout(timeoutId);
  }, [searchInput]);

  const params: RequestListParams = {
    page,
    pageSize,
    search: debouncedSearch || undefined,
    status: status || undefined,
    category: category || undefined,
    sortBy,
    sortDirection,
  };

  const { data, isLoading, isFetching, isError, refetch } = useRequests(params);

  function handleStatusChange(value: RequestStatus | "") {
    setStatus(value);
    setPage(1);
  }

  function handleCategoryChange(value: RequestCategory | "") {
    setCategory(value);
    setPage(1);
  }

  function handlePageSizeChange(value: number) {
    setPageSize(value);
    setPage(1);
  }

  function handleSortChange(column: "title" | "status" | "createdAt" | "updatedAt") {
    if (sortBy === column) {
      setSortDirection((current) => (current === "asc" ? "desc" : "asc"));
    } else {
      setSortBy(column);
      setSortDirection("asc");
    }

    setPage(1);
  }

  function clearFilters() {
    setSearchInput("");
    setDebouncedSearch("");
    setStatus("");
    setCategory("");
    setPage(1);
  }

  if (isLoading) {
    return (
      <Stack spacing={3}>
        <PageHeading onCreate={() => navigate("/requests/new")} />

        <RequestListSkeleton />
      </Stack>
    );
  }

  return (
    <Stack spacing={3}>
      <PageHeading onCreate={() => navigate("/requests/new")} />

      <Paper variant="outlined" sx={{ p: 2 }}>
        <RequestFilters
          search={searchInput}
          status={status}
          category={category}
          onSearchChange={setSearchInput}
          onStatusChange={handleStatusChange}
          onCategoryChange={handleCategoryChange}
          onClear={clearFilters}
        />
      </Paper>

      {isError && (
        <Alert
          severity="error"
          action={
            <Button color="inherit" size="small" onClick={() => refetch()}>
              Retry
            </Button>
          }
        >
          Unable to load requests.
        </Alert>
      )}

      {!isError && data && data.items.length === 0 && (
        <Paper variant="outlined" sx={{ p: 6, textAlign: "center" }}>
          <Typography variant="h6">No requests found</Typography>

          <Typography color="text.secondary" sx={{ mt: 1 }}>
            {searchInput || status || category
              ? "Try changing or clearing your filters."
              : "Create your first request to get started."}
          </Typography>

          {!searchInput && !status && !category && (
            <Button variant="contained" startIcon={<AddIcon />} sx={{ mt: 3 }} onClick={() => navigate("/requests/new")}>
              Create request
            </Button>
          )}
        </Paper>
      )}

      {!isError && data && data.items.length > 0 && (
        <RequestsTable
          requests={data.items}
          page={data.page}
          pageSize={data.pageSize}
          totalCount={data.totalCount}
          sortBy={sortBy}
          sortDirection={sortDirection}
          isFetching={isFetching}
          onPageChange={setPage}
          onPageSizeChange={handlePageSizeChange}
          onSortChange={handleSortChange}
        />
      )}
    </Stack>
  );
}

type PageHeadingProps = {
  onCreate: () => void;
};

function PageHeading({ onCreate }: PageHeadingProps) {
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: {
          xs: "flex-start",
          sm: "center",
        },
        flexDirection: {
          xs: "column",
          sm: "row",
        },
        gap: 2,
      }}
    >
      <Box>
        <Typography variant="h4">Requests</Typography>

        <Typography color="text.secondary">Create and track workflow requests.</Typography>
      </Box>

      <Button variant="contained" startIcon={<AddIcon />} onClick={onCreate}>
        New request
      </Button>
    </Box>
  );
}
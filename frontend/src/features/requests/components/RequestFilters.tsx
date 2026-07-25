import { Box, Button, FormControl, InputLabel, MenuItem, Select, TextField } from "@mui/material";
import type { RequestCategory, RequestStatus } from "../requestTypes";

type RequestFiltersProps = {
  search: string;
  status: RequestStatus | "";
  category: RequestCategory | "";
  onSearchChange: (value: string) => void;
  onStatusChange: (value: RequestStatus | "") => void;
  onCategoryChange: (value: RequestCategory | "") => void;
  onClear: () => void;
};

const statuses: RequestStatus[] = [
  "Draft",
  "Submitted",
  "UnderReview",
  "Approved",
  "Rejected",
  "Cancelled",
];

const categories: RequestCategory[] = [
  "Equipment",
  "Training",
  "SoftwareAccess",
  "Expense",
  "Leave",
  "Other",
];

export function RequestFilters({
  search,
  status,
  category,
  onSearchChange,
  onStatusChange,
  onCategoryChange,
  onClear,
}: RequestFiltersProps) {
  const hasFilters = Boolean(search || status || category);

  return (
    <Box
      sx={{
        display: "flex",
        flexWrap: "wrap",
        gap: 2,
        alignItems: "center",
      }}
    >
      <TextField
        label="Search requests"
        value={search}
        onChange={(event) => onSearchChange(event.target.value)}
        size="small"
        sx={{ minWidth: 260, flexGrow: 1 }}
        inputProps={{
          "aria-label": "Search requests",
        }}
      />

      <FormControl size="small" sx={{ minWidth: 170 }}>
        <InputLabel id="request-status-label">Status</InputLabel>

        <Select
          labelId="request-status-label"
          label="Status"
          value={status}
          onChange={(event) => onStatusChange(event.target.value as RequestStatus | "")}
        >
          <MenuItem value="">All statuses</MenuItem>

          {statuses.map((item) => (
            <MenuItem key={item} value={item}>
              {item === "UnderReview" ? "Under review" : item}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      <FormControl size="small" sx={{ minWidth: 180 }}>
        <InputLabel id="request-category-label">Category</InputLabel>

        <Select
          labelId="request-category-label"
          label="Category"
          value={category}
          onChange={(event) => onCategoryChange(event.target.value as RequestCategory | "")}
        >
          <MenuItem value="">All categories</MenuItem>

          {categories.map((item) => (
            <MenuItem key={item} value={item}>
              {item === "SoftwareAccess" ? "Software access" : item}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      <Button variant="text" onClick={onClear} disabled={!hasFilters}>
        Clear filters
      </Button>
    </Box>
  );
}
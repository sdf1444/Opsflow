import { Paper, Stack, Typography } from "@mui/material";
import type { RequestAuditEntry } from "../requestTypes";

type AuditTimelineProps = {
  entries: RequestAuditEntry[];
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

function toLabel(action: string): string {
  if (!action) {
    return "Activity";
  }

  return action
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/_/g, " ")
    .trim();
}

export function AuditTimeline({ entries }: AuditTimelineProps) {
  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}>
      <Stack spacing={2}>
        <Typography variant="h6">Activity</Typography>

        {entries.length === 0 ? (
          <Typography color="text.secondary">No activity recorded.</Typography>
        ) : (
          <Stack spacing={1.75}>
            {entries.map((entry) => (
              <Stack key={entry.id} spacing={0.5} sx={{ borderLeft: 2, borderColor: "divider", pl: 1.5 }}>
                <Typography variant="body2" fontWeight={600}>
                  {toLabel(entry.action)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  By {entry.performedBy.name} at {formatDate(entry.createdAt)}
                </Typography>
                {entry.description && <Typography variant="body2">{entry.description}</Typography>}
              </Stack>
            ))}
          </Stack>
        )}
      </Stack>
    </Paper>
  );
}

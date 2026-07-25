import { Avatar, List, ListItem, ListItemAvatar, ListItemText, Paper, Stack, Typography } from "@mui/material";
import type { RequestComment } from "../requestTypes";

type CommentsListProps = {
  comments: RequestComment[];
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

function initials(name: string): string {
  const parts = name.trim().split(/\s+/);

  if (parts.length === 0) {
    return "?";
  }

  return parts
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");
}

export function CommentsList({ comments }: CommentsListProps) {
  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}>
      <Stack spacing={2}>
        <Typography variant="h6">Comments</Typography>

        {comments.length === 0 ? (
          <Typography color="text.secondary">No comments yet.</Typography>
        ) : (
          <List disablePadding>
            {comments.map((comment) => (
              <ListItem key={comment.id} alignItems="flex-start" disableGutters sx={{ py: 1.25 }}>
                <ListItemAvatar>
                  <Avatar>{initials(comment.author.name)}</Avatar>
                </ListItemAvatar>
                <ListItemText
                  primary={
                    <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap" alignItems="center">
                      <Typography variant="body2" fontWeight={600}>
                        {comment.author.name}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {comment.author.email}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {formatDate(comment.createdAt)}
                      </Typography>
                    </Stack>
                  }
                  secondary={<Typography sx={{ mt: 0.75, whiteSpace: "pre-wrap" }}>{comment.content}</Typography>}
                />
              </ListItem>
            ))}
          </List>
        )}
      </Stack>
    </Paper>
  );
}

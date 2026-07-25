import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { Alert, Button, Container, Paper, Stack, Typography } from "@mui/material";
import { useState } from "react";
import { Link as RouterLink, useParams } from "react-router-dom";
import { toApiError } from "../../api/apiError";
import { useAuth } from "../auth/useAuth";
import { AddCommentForm } from "./components/AddCommentForm";
import { AuditTimeline } from "./components/AuditTimeline";
import { CommentsList } from "./components/CommentsList";
import { RequestActions } from "./components/RequestActions";
import { RequestDetailSkeleton } from "./components/RequestDetailSkeleton";
import { RequestDetailsCard } from "./components/RequestDetailsCard";
import { useAddRequestComment } from "./detail/requestActionHooks";
import { useRequestDetail } from "./detail/requestDetailHooks";

export default function RequestDetailPage() {
  const { id = "" } = useParams<{ id: string }>();
  const { user } = useAuth();

  const requestQuery = useRequestDetail(id);
  const addCommentMutation = useAddRequestComment(id);
  const [commentError, setCommentError] = useState<string | null>(null);

  if (!id) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error">Invalid request ID.</Alert>
      </Container>
    );
  }

  if (requestQuery.isLoading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <RequestDetailSkeleton />
      </Container>
    );
  }

  if (requestQuery.isError) {
    const apiError = toApiError(requestQuery.error);
    const isBackendUnavailable = apiError.status === undefined;
    const title = isBackendUnavailable
      ? "Unable to load this request"
      : apiError.status === 404
        ? "Request not found"
        : apiError.status === 403
          ? "Access denied"
          : "Unable to load request";
    const severity = apiError.status === 404 ? "warning" : "error";

    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Stack spacing={2}>
          <Button component={RouterLink} to="/requests" startIcon={<ArrowBackIcon />} sx={{ alignSelf: "flex-start" }}>
            Back to requests
          </Button>

          <Paper variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}>
            <Stack spacing={1}>
              <Alert severity={severity}>
                <Typography variant="subtitle1" fontWeight={600}>
                  {title}
                </Typography>
                {!isBackendUnavailable && <Typography variant="body2">{apiError.message}</Typography>}
              </Alert>

              <Button variant="outlined" onClick={() => requestQuery.refetch()} sx={{ alignSelf: "flex-start" }}>
                Retry
              </Button>
            </Stack>
          </Paper>
        </Stack>
      </Container>
    );
  }

  const request = requestQuery.data;
  if (!request || !user) {
    return null;
  }

  async function handleAddComment(content: string) {
    setCommentError(null);

    try {
      await addCommentMutation.mutateAsync({ content });
    } catch (error: unknown) {
      setCommentError(toApiError(error).message);
    }
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={2.5}>
        <Button component={RouterLink} to="/requests" startIcon={<ArrowBackIcon />} sx={{ alignSelf: "flex-start" }}>
          Back to requests
        </Button>

        <RequestDetailsCard request={request} />

        <Paper variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}>
          <RequestActions request={request} currentUserId={user.id} currentUserRole={user.role} />
        </Paper>

        <AddCommentForm isSubmitting={addCommentMutation.isPending} onSubmit={handleAddComment} />

        {commentError && <Alert severity="error">{commentError}</Alert>}

        <CommentsList comments={request.comments} />

        <AuditTimeline entries={request.auditLogs} />
      </Stack>
    </Container>
  );
}

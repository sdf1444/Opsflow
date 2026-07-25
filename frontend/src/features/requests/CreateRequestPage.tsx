import { useState } from "react";
import { Container, Paper, Stack, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { useCreateRequest } from "./create/useCreateRequest";
import { RequestForm } from "./components/RequestForm";
import type { CreateRequestForm } from "./create/createRequestSchema";

export default function CreateRequestPage() {
  const navigate = useNavigate();
  const createRequestMutation = useCreateRequest();
  const [savingMode, setSavingMode] = useState<"draft" | "submit" | null>(null);

  function handleCancel() {
    navigate("/requests");
  }

  function handleCreate(values: CreateRequestForm, submit: boolean) {
    setSavingMode(submit ? "submit" : "draft");

    createRequestMutation.mutate(
      {
        ...values,
        submit,
      },
      {
        onSuccess: (response) => {
          navigate("/requests", {
            replace: true,
            state: {
              flashMessage:
                response.status === "Submitted"
                  ? "Request submitted for review."
                  : "Request created successfully.",
            },
          });
        },
        onSettled: () => {
          setSavingMode(null);
        },
      },
    );
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Stack spacing={3}>
        <Stack spacing={0.5}>
          <Typography variant="h4">New Request</Typography>
          <Typography color="text.secondary">Create a draft or submit a request immediately.</Typography>
        </Stack>

        <Paper variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}>
          <RequestForm
            onCancel={handleCancel}
            onSaveDraft={(values) => handleCreate(values, false)}
            onSubmit={(values) => handleCreate(values, true)}
            isSaving={createRequestMutation.isPending}
            savingMode={savingMode}
            submitError={createRequestMutation.isError ? "Unable to create request." : null}
          />
        </Paper>
      </Stack>
    </Container>
  );
}
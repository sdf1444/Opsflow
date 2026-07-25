import {
  Alert,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { createRequestSchema, type CreateRequestForm } from "../create/createRequestSchema";
import type { RequestCategory } from "../requestTypes";
import { RequestFormActions } from "./RequestFormActions";

type RequestFormProps = {
  onCancel: () => void;
  onSaveDraft: (values: CreateRequestForm) => void;
  onSubmit: (values: CreateRequestForm) => void;
  isSaving: boolean;
  savingMode: "draft" | "submit" | null;
  submitError: string | null;
};

const categoryOptions: RequestCategory[] = [
  "Equipment",
  "Training",
  "SoftwareAccess",
  "Expense",
  "Leave",
  "Other",
];

export function RequestForm({
  onCancel,
  onSaveDraft,
  onSubmit,
  isSaving,
  savingMode,
  submitError,
}: RequestFormProps) {
  const {
    control,
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<CreateRequestForm>({
    resolver: zodResolver(createRequestSchema),
    defaultValues: {
      title: "",
      description: "",
      category: "Equipment",
    },
    mode: "onSubmit",
  });

  const titleValue = watch("title");
  const descriptionValue = watch("description");

  const handleSaveDraft = handleSubmit((values) => onSaveDraft(values));
  const handleSubmitRequest = handleSubmit((values) => onSubmit(values));

  return (
    <Stack spacing={3}>
      {submitError && <Alert severity="error">{submitError}</Alert>}

      <TextField
        label="Title"
        fullWidth
        disabled={isSaving}
        error={Boolean(errors.title)}
        helperText={errors.title?.message ?? `${titleValue.length}/100 characters`}
        {...register("title")}
      />

      <FormControl fullWidth error={Boolean(errors.category)} disabled={isSaving}>
        <InputLabel id="request-category-label">Category</InputLabel>
        <Controller
          control={control}
          name="category"
          render={({ field }) => (
            <Select {...field} labelId="request-category-label" label="Category">
              {categoryOptions.map((item) => (
                <MenuItem key={item} value={item}>
                  {item === "SoftwareAccess" ? "Software access" : item}
                </MenuItem>
              ))}
            </Select>
          )}
        />
        <Typography variant="caption" color="error" sx={{ minHeight: 18, mt: 0.5 }}>
          {errors.category?.message}
        </Typography>
      </FormControl>

      <TextField
        label="Description"
        fullWidth
        multiline
        rows={8}
        disabled={isSaving}
        error={Boolean(errors.description)}
        helperText={errors.description?.message ?? `${descriptionValue.length}/2000 characters`}
        {...register("description")}
      />

      <RequestFormActions
        onCancel={onCancel}
        onSaveDraft={handleSaveDraft}
        onSubmit={handleSubmitRequest}
        isSaving={isSaving}
        savingMode={savingMode}
      />
    </Stack>
  );
}
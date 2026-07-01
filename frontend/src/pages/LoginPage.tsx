import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { zodResolver } from "@hookform/resolvers/zod";
import { Navigate } from "react-router-dom";
import { Controller, useForm } from "react-hook-form";
import { loginSchema, type LoginForm } from "../features/auth/loginSchema";
import { useAuth } from "../features/auth/useAuth";
import { useLogin } from "../features/auth/useLogin";

export default function LoginPage() {
  const { isAuthenticated, loading } = useAuth();
  const loginMutation = useLogin();

  const { control, handleSubmit } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  if (loading) {
    return (
      <Box
        sx={{
          minHeight: "100vh",
          display: "grid",
          placeItems: "center",
          bgcolor: "background.default",
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  const onSubmit = handleSubmit((values) => {
    loginMutation.mutate(values);
  });

  const isBusy = loginMutation.isPending;

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "grid",
        placeItems: "center",
        px: 2,
        bgcolor: "background.default",
      }}
    >
      <Card sx={{ width: "100%", maxWidth: 420 }}>
        <CardContent sx={{ p: 4 }}>
          <Stack component="form" spacing={2} onSubmit={onSubmit} noValidate>
            <Box>
              <Typography variant="h4" sx={{ fontWeight: 700 }}>
                OpsFlow
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                Sign in to continue
              </Typography>
            </Box>

            <Controller
              name="email"
              control={control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  type="email"
                  label="Email"
                  autoComplete="email"
                  disabled={isBusy}
                  error={Boolean(fieldState.error)}
                  helperText={fieldState.error?.message}
                  fullWidth
                />
              )}
            />

            <Controller
              name="password"
              control={control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  type="password"
                  label="Password"
                  autoComplete="current-password"
                  disabled={isBusy}
                  error={Boolean(fieldState.error)}
                  helperText={fieldState.error?.message}
                  fullWidth
                />
              )}
            />

            {loginMutation.error ? <Alert severity="error">{loginMutation.error.message}</Alert> : null}

            <Button type="submit" variant="contained" size="large" disabled={isBusy} fullWidth>
              {isBusy ? "Signing in..." : "Sign In"}
            </Button>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  );
}

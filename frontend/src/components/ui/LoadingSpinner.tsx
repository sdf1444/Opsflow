import { Box, CircularProgress, Typography } from "@mui/material";

type Props = {
  label?: string;
  fullPage?: boolean;
};

export default function LoadingSpinner({ label = "Loading...", fullPage = false }: Props) {
  return (
    <Box
      sx={{
        minHeight: fullPage ? "100vh" : 120,
        width: "100%",
        display: "grid",
        placeItems: "center",
        gap: 1,
      }}
    >
      <CircularProgress />
      {label ? (
        <Typography variant="body2" color="text.secondary">
          {label}
        </Typography>
      ) : null}
    </Box>
  );
}

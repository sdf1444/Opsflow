import { createTheme } from "@mui/material/styles";

export const theme = createTheme({
  palette: {
    mode: "light",
    primary: {
      main: "#1565c0",
    },
    background: {
      default: "#f1f4f8",
      paper: "#ffffff",
    },
  },
  shape: {
    borderRadius: 14,
  },
  spacing: 8,
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 14,
        },
      },
    },
  },
});

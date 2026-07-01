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
    borderRadius: 12,
  },
  typography: {
    fontFamily: "Roboto, Helvetica, Arial, sans-serif",
  },
  spacing: 8,
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          textTransform: "none",
        },
      },
    },
  },
});

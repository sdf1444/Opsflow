import { useState } from "react";
import { Box, Container, Toolbar, useMediaQuery, useTheme } from "@mui/material";
import { Outlet } from "react-router-dom";
import AppHeader from "../components/navigation/AppHeader";
import AppSidebar from "../components/navigation/AppSidebar";

export default function MainLayout() {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));
  const [mobileOpen, setMobileOpen] = useState(false);

  return (
    <Box sx={{ display: "flex", minHeight: "100vh", bgcolor: "background.default" }}>
      <AppHeader isMobile={isMobile} onMenuClick={() => setMobileOpen(true)} />

      <AppSidebar
        isMobile={isMobile}
        mobileOpen={mobileOpen}
        onMobileClose={() => setMobileOpen(false)}
      />

      <Box component="main" sx={{ flexGrow: 1 }}>
        <Toolbar />
        <Container maxWidth="xl" sx={{ py: 3 }}>
          <Box sx={{ minHeight: 24, mb: 2 }} />
          <Outlet />
        </Container>
      </Box>
    </Box>
  );
}

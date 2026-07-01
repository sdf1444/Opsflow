import {
  AppBar,
  Box,
  Drawer,
  List,
  ListItemButton,
  ListItemText,
  Toolbar,
  Typography,
} from "@mui/material";
import { Link as RouterLink, Outlet } from "react-router-dom";

const drawerWidth = 240;

export default function MainLayout() {
  return (
    <Box sx={{ display: "flex", minHeight: "100vh", bgcolor: "background.default" }}>
      <AppBar
        position="fixed"
        color="inherit"
        elevation={0}
        sx={{ borderBottom: "1px solid", borderColor: "divider", zIndex: (t) => t.zIndex.drawer + 1 }}
      >
        <Toolbar>
          <Typography variant="h6" sx={{ fontWeight: 700 }}>
            OpsFlow
          </Typography>
        </Toolbar>
      </AppBar>

      <Drawer
        variant="permanent"
        sx={{
          width: drawerWidth,
          flexShrink: 0,
          "& .MuiDrawer-paper": {
            width: drawerWidth,
            boxSizing: "border-box",
            pt: 8,
          },
        }}
      >
        <List>
          <ListItemButton component={RouterLink} to="/dashboard">
            <ListItemText primary="Dashboard" />
          </ListItemButton>
          <ListItemButton component={RouterLink} to="/requests">
            <ListItemText primary="Requests" />
          </ListItemButton>
          <ListItemButton component={RouterLink} to="/approvals">
            <ListItemText primary="Approvals" />
          </ListItemButton>
          <ListItemButton component={RouterLink} to="/profile">
            <ListItemText primary="Profile" />
          </ListItemButton>
        </List>
      </Drawer>

      <Box component="main" sx={{ flexGrow: 1, p: 3, pt: 10 }}>
        <Outlet />
      </Box>
    </Box>
  );
}

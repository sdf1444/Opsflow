import { useState } from "react";
import MenuIcon from "@mui/icons-material/Menu";
import {
  AppBar,
  Avatar,
  Box,
  IconButton,
  Menu,
  MenuItem,
  Toolbar,
  Typography,
} from "@mui/material";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../features/auth/useAuth";
import { DRAWER_WIDTH } from "./AppSidebar";

type AppHeaderProps = {
  isMobile: boolean;
  onMenuClick: () => void;
};

function initialsFromName(name?: string) {
  if (!name) {
    return "U";
  }

  const parts = name.trim().split(/\s+/).slice(0, 2);
  return parts.map((part) => part[0]?.toUpperCase() ?? "").join("") || "U";
}

export default function AppHeader({ isMobile, onMenuClick }: AppHeaderProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const menuOpen = Boolean(anchorEl);

  const handleProfileClick = () => {
    setAnchorEl(null);
    navigate("/profile");
  };

  const handleLogoutClick = () => {
    setAnchorEl(null);
    logout();
  };

  return (
    <AppBar
      position="fixed"
      color="inherit"
      elevation={0}
      sx={{
        borderBottom: "1px solid",
        borderColor: "divider",
        width: isMobile ? "100%" : `calc(100% - ${DRAWER_WIDTH}px)`,
        ml: isMobile ? 0 : `${DRAWER_WIDTH}px`,
      }}
    >
      <Toolbar>
        {isMobile ? (
          <IconButton edge="start" color="inherit" onClick={onMenuClick} sx={{ mr: 1 }}>
            <MenuIcon />
          </IconButton>
        ) : null}

        <Typography variant="h6" sx={{ fontWeight: 700 }}>
          OpsFlow
        </Typography>

        <Box sx={{ flexGrow: 1 }} />

        <IconButton color="inherit" onClick={(event) => setAnchorEl(event.currentTarget)}>
          <Avatar sx={{ width: 34, height: 34 }}>{initialsFromName(user?.name)}</Avatar>
        </IconButton>

        <Box sx={{ ml: 1, display: { xs: "none", sm: "block" } }}>
          <Typography variant="body2" sx={{ lineHeight: 1.2, fontWeight: 600 }}>
            {user?.name ?? "User"}
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.2 }}>
            {user?.role ?? "Employee"}
          </Typography>
        </Box>

        <Menu
          anchorEl={anchorEl}
          open={menuOpen}
          onClose={() => setAnchorEl(null)}
          anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
          transformOrigin={{ vertical: "top", horizontal: "right" }}
        >
          <MenuItem onClick={handleProfileClick}>Profile</MenuItem>
          <MenuItem onClick={handleLogoutClick}>Logout</MenuItem>
        </Menu>
      </Toolbar>
    </AppBar>
  );
}

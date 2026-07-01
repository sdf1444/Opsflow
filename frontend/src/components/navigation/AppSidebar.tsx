import { Box, Divider, Drawer, List, Toolbar } from "@mui/material";
import { useLocation } from "react-router-dom";
import { navigation } from "../../constants/navigation";
import { useAuth } from "../../features/auth/useAuth";
import NavItem from "./NavItem";

export const DRAWER_WIDTH = 240;

type AppSidebarProps = {
  mobileOpen: boolean;
  onMobileClose: () => void;
  isMobile: boolean;
};

export default function AppSidebar({ mobileOpen, onMobileClose, isMobile }: AppSidebarProps) {
  const location = useLocation();
  const { user } = useAuth();

  const role = user?.role;
  const visibleNavigation = navigation.filter((item) => (role ? item.roles.includes(role) : false));

  const drawerContent = (
    <>
      <Toolbar />
      <Divider />
      <Box sx={{ px: 1, py: 2 }}>
        <List disablePadding>
          {visibleNavigation.map((item) => (
            <NavItem
              key={item.path}
              item={item}
              currentPath={location.pathname}
              onClick={isMobile ? onMobileClose : undefined}
            />
          ))}
        </List>
      </Box>
    </>
  );

  if (isMobile) {
    return (
      <Drawer
        variant="temporary"
        open={mobileOpen}
        onClose={onMobileClose}
        ModalProps={{ keepMounted: true }}
        sx={{
          "& .MuiDrawer-paper": {
            width: DRAWER_WIDTH,
            boxSizing: "border-box",
          },
        }}
      >
        {drawerContent}
      </Drawer>
    );
  }

  return (
    <Drawer
      variant="permanent"
      open
      sx={{
        width: DRAWER_WIDTH,
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: DRAWER_WIDTH,
          boxSizing: "border-box",
        },
      }}
    >
      {drawerContent}
    </Drawer>
  );
}

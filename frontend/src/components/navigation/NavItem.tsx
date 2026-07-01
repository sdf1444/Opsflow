import { ListItemButton, ListItemIcon, ListItemText } from "@mui/material";
import { Link as RouterLink } from "react-router-dom";
import type { NavigationItem } from "../../constants/navigation";

type NavItemProps = {
  item: NavigationItem;
  currentPath: string;
  onClick?: () => void;
};

function isItemActive(currentPath: string, itemPath: string) {
  return currentPath === itemPath || currentPath.startsWith(`${itemPath}/`);
}

export default function NavItem({ item, currentPath, onClick }: NavItemProps) {
  const Icon = item.icon;
  const selected = isItemActive(currentPath, item.path);

  return (
    <ListItemButton
      component={RouterLink}
      to={item.path}
      selected={selected}
      onClick={onClick}
      sx={{ borderRadius: 1, mx: 1 }}
    >
      <ListItemIcon>
        <Icon fontSize="small" />
      </ListItemIcon>
      <ListItemText primary={item.title} />
    </ListItemButton>
  );
}

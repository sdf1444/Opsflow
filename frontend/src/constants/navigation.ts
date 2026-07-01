import type { SvgIconComponent } from "@mui/icons-material";
import DashboardIcon from "@mui/icons-material/Dashboard";
import AssignmentIcon from "@mui/icons-material/Assignment";
import ApprovalIcon from "@mui/icons-material/FactCheck";
import AdminIcon from "@mui/icons-material/AdminPanelSettings";
import type { UserRole } from "../features/auth/authTypes";

export type NavigationItem = {
  title: string;
  icon: SvgIconComponent;
  path: string;
  roles: UserRole[];
};

export const navigation: NavigationItem[] = [
  {
    title: "Dashboard",
    icon: DashboardIcon,
    path: "/dashboard",
    roles: ["Employee", "Manager", "Admin"],
  },
  {
    title: "Requests",
    icon: AssignmentIcon,
    path: "/requests",
    roles: ["Employee", "Manager", "Admin"],
  },
  {
    title: "Approvals",
    icon: ApprovalIcon,
    path: "/approvals",
    roles: ["Manager", "Admin"],
  },
  {
    title: "Admin",
    icon: AdminIcon,
    path: "/admin",
    roles: ["Admin"],
  },
];

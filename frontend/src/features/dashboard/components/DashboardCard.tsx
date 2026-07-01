import { Box, Typography } from "@mui/material";
import type { ReactNode } from "react";
import { Card } from "../../../components/ui";

type DashboardCardProps = {
  title: string;
  count: number;
  icon: ReactNode;
  color: string;
};

export default function DashboardCard({ title, count, icon, color }: DashboardCardProps) {
  return (
    <Card sx={{ height: "100%" }} contentSx={{ height: "100%" }}>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", gap: 2 }}>
        <Box>
          <Typography variant="body2" color="text.secondary">
            {title}
          </Typography>
          <Typography variant="h4" sx={{ mt: 1, fontWeight: 700 }}>
            {count}
          </Typography>
        </Box>

        <Box
          sx={{
            width: 42,
            height: 42,
            borderRadius: 1.5,
            display: "grid",
            placeItems: "center",
            color,
            bgcolor: "action.hover",
          }}
        >
          {icon}
        </Box>
      </Box>
    </Card>
  );
}

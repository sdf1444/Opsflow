import { Chip, type ChipProps } from "@mui/material";

type StatusTone = "default" | "success" | "warning" | "error" | "info";

const toneByStatus: Record<string, StatusTone> = {
  draft: "default",
  submitted: "info",
  pending: "warning",
  approved: "success",
  rejected: "error",
  cancelled: "default",
};

type Props = {
  status: string;
  label?: string;
  size?: ChipProps["size"];
};

export default function StatusBadge({ status, label, size = "small" }: Props) {
  const tone = toneByStatus[status.toLowerCase()] ?? "default";

  return (
    <Chip
      size={size}
      label={label ?? status}
      color={tone === "default" ? undefined : tone}
      variant={tone === "default" ? "outlined" : "filled"}
      sx={{ fontWeight: 600 }}
    />
  );
}

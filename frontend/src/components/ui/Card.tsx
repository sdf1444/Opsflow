import {
  Card as MuiCard,
  CardContent,
  type CardProps as MuiCardProps,
  type SxProps,
  type Theme,
} from "@mui/material";

type Props = MuiCardProps & {
  padded?: boolean;
  contentSx?: SxProps<Theme>;
};

export default function Card({ children, padded = true, contentSx, ...props }: Props) {
  if (!padded) {
    return <MuiCard {...props}>{children}</MuiCard>;
  }

  return (
    <MuiCard {...props}>
      <CardContent sx={contentSx}>{children}</CardContent>
    </MuiCard>
  );
}

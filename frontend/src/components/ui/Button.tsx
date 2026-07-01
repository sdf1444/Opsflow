import { Button as MuiButton, CircularProgress, type ButtonProps } from "@mui/material";

type Props = ButtonProps & {
  isLoading?: boolean;
  loadingText?: string;
};

export default function Button({
  children,
  isLoading = false,
  loadingText = "Loading...",
  disabled,
  ...props
}: Props) {
  const content = isLoading ? loadingText : children;

  return (
    <MuiButton disabled={disabled || isLoading} {...props}>
      {isLoading ? <CircularProgress size={18} color="inherit" sx={{ mr: 1 }} /> : null}
      {content}
    </MuiButton>
  );
}

import type { PropsWithChildren } from "react";
import { AuthSessionProvider } from "../providers/auth/AuthSessionProvider";

export function AuthProvider({ children }: PropsWithChildren) {
  return <AuthSessionProvider>{children}</AuthSessionProvider>;
}

import { createContext } from "react";

export type AuthUser = {
  id?: string;
  email?: string;
  name?: string;
  role?: string;
};

export type AuthContextValue = {
  user: AuthUser | null;
  token: string | null;
  login: (nextToken: string, nextUser?: AuthUser | null) => void;
  logout: () => void;
  isAuthenticated: boolean;
};

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

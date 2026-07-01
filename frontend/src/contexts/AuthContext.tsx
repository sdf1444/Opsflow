import {
  useCallback,
  useMemo,
  useState,
  type PropsWithChildren,
} from "react";
import {
  AuthContext,
  type AuthContextValue,
  type AuthUser,
} from "./auth-context";

export function AuthProvider({ children }: PropsWithChildren) {
  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<AuthUser | null>(null);

  const login = useCallback((nextToken: string, nextUser?: AuthUser | null) => {
    setToken(nextToken);
    setUser(nextUser ?? null);
  }, []);

  const logout = useCallback(() => {
    setToken(null);
    setUser(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      login,
      logout,
      isAuthenticated: Boolean(token),
    }),
    [user, token, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

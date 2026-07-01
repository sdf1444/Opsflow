import {
  useCallback,
  useEffect,
  useMemo,
  useReducer,
  type PropsWithChildren,
} from "react";
import {
  AuthContext,
  type AuthContextValue,
  type AuthUser,
} from "../../contexts/auth-context";
import { api } from "../../api/client";
import {
  authSessionReducer,
  initialState,
  readSessionFromStorage,
  writeSessionToStorage,
} from "./auth-session";

export function AuthSessionProvider({ children }: PropsWithChildren) {
  const [session, dispatch] = useReducer(authSessionReducer, initialState);

  useEffect(() => {
    dispatch({ type: "restore", payload: readSessionFromStorage() });
  }, []);

  useEffect(() => {
    writeSessionToStorage(session);

    if (session.token) {
      api.defaults.headers.common.Authorization = `Bearer ${session.token}`;
      return;
    }

    delete api.defaults.headers.common.Authorization;
  }, [session]);

  const login = useCallback((nextToken: string, nextUser?: AuthUser | null) => {
    dispatch({
      type: "login",
      payload: {
        token: nextToken,
        user: nextUser ?? null,
      },
    });
  }, []);

  const logout = useCallback(() => {
    dispatch({ type: "logout" });
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session.user,
      token: session.token,
      login,
      logout,
      isAuthenticated: Boolean(session.token),
    }),
    [session, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

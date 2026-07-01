import {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from "react";
import { useNavigate } from "react-router-dom";
import { clearAccessToken, getAccessToken, setAccessToken } from "../../api/authToken";
import { getCurrentUser } from "./authApi";
import type { CurrentUserResponse } from "./authTypes";

type AuthContextValue = {
  user: CurrentUserResponse | null;
  token: string | null;
  login: (nextToken: string, nextUser: CurrentUserResponse) => void;
  logout: () => void;
  isAuthenticated: boolean;
  loading: boolean;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: PropsWithChildren) {
  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<CurrentUserResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const existingToken = getAccessToken();
    if (!existingToken) {
      setLoading(false);
      return;
    }

    let isMounted = true;
    setToken(existingToken);

    getCurrentUser()
      .then((currentUser) => {
        if (!isMounted) {
          return;
        }

        setUser(currentUser);
      })
      .catch(() => {
        if (!isMounted) {
          return;
        }

        clearAccessToken();
        setToken(null);
        setUser(null);
      })
      .finally(() => {
        if (isMounted) {
          setLoading(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, []);

  const login = useCallback((nextToken: string, nextUser: CurrentUserResponse) => {
    setAccessToken(nextToken);
    setToken(nextToken);
    setUser(nextUser);
  }, []);

  const logout = useCallback(() => {
    clearAccessToken();
    setToken(null);
    setUser(null);
    navigate("/login", { replace: true });
  }, [navigate]);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      login,
      logout,
      isAuthenticated: Boolean(token && user),
      loading,
    }),
    [user, token, login, logout, loading],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export { AuthContext };

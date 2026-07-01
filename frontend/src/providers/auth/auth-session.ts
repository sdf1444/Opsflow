import type { AuthUser } from "../../contexts/auth-context";
import {
  clearAccessToken,
  getAccessToken,
  setAccessToken,
} from "../../api/authToken";

const AUTH_USER_KEY = "opsflow.auth.user";

type AuthSessionState = {
  token: string | null;
  user: AuthUser | null;
};

type LoginPayload = {
  token: string;
  user: AuthUser | null;
};

type AuthSessionAction =
  | { type: "login"; payload: LoginPayload }
  | { type: "logout" }
  | { type: "restore"; payload: AuthSessionState };

const initialState: AuthSessionState = {
  token: null,
  user: null,
};

function authSessionReducer(
  state: AuthSessionState,
  action: AuthSessionAction,
): AuthSessionState {
  switch (action.type) {
    case "login":
      return {
        token: action.payload.token,
        user: action.payload.user,
      };
    case "logout":
      return initialState;
    case "restore":
      return {
        token: action.payload.token,
        user: action.payload.user,
      };
    default:
      return state;
  }
}

function parseStoredSession(value: string | null): AuthSessionState {
  const token = getAccessToken();
  if (!value) {
    return initialState;
  }

  try {
    const parsed = JSON.parse(value) as Partial<AuthUser>;
    return {
      token,
      user: parsed ?? null,
    };
  } catch {
    return {
      token,
      user: null,
    };
  }
}

function readSessionFromStorage(): AuthSessionState {
  if (typeof window === "undefined") {
    return initialState;
  }

  return parseStoredSession(window.localStorage.getItem(AUTH_USER_KEY));
}

function writeSessionToStorage(state: AuthSessionState) {
  if (typeof window === "undefined") {
    return;
  }

  if (!state.token) {
    clearAccessToken();
    window.localStorage.removeItem(AUTH_USER_KEY);
    return;
  }

  setAccessToken(state.token);
  window.localStorage.setItem(AUTH_USER_KEY, JSON.stringify(state.user));
}

export {
  authSessionReducer,
  initialState,
  readSessionFromStorage,
  writeSessionToStorage,
};
export type { AuthSessionState };

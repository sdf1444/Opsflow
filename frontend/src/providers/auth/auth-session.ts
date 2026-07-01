import type { AuthUser } from "../../contexts/auth-context";

const AUTH_SESSION_KEY = "opsflow.auth.session";

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
  if (!value) {
    return initialState;
  }

  try {
    const parsed = JSON.parse(value) as Partial<AuthSessionState>;
    return {
      token: typeof parsed.token === "string" ? parsed.token : null,
      user: parsed.user ?? null,
    };
  } catch {
    return initialState;
  }
}

function readSessionFromStorage(): AuthSessionState {
  if (typeof window === "undefined") {
    return initialState;
  }

  return parseStoredSession(window.localStorage.getItem(AUTH_SESSION_KEY));
}

function writeSessionToStorage(state: AuthSessionState) {
  if (typeof window === "undefined") {
    return;
  }

  if (!state.token) {
    window.localStorage.removeItem(AUTH_SESSION_KEY);
    return;
  }

  window.localStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(state));
}

export {
  authSessionReducer,
  initialState,
  readSessionFromStorage,
  writeSessionToStorage,
};
export type { AuthSessionState };

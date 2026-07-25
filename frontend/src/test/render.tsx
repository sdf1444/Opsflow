import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { CssBaseline, ThemeProvider } from "@mui/material";
import { MemoryRouter } from "react-router-dom";
import { render, type RenderOptions } from "@testing-library/react";
import type { PropsWithChildren, ReactElement } from "react";
import { AuthContext } from "../features/auth/authContext";
import type { CurrentUserResponse } from "../features/auth/authTypes";
import { theme } from "../styles/theme";
import { buildUser } from "./dataBuilders";

type ProvidersOptions = {
  route?: string;
  user?: CurrentUserResponse | null;
  token?: string | null;
  queryClient?: QueryClient;
};

export function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  });
}

export function renderWithProviders(
  ui: ReactElement,
  options?: ProvidersOptions & Omit<RenderOptions, "wrapper">,
) {
  const {
    route = "/",
    user = buildUser(),
    token = "test-token",
    queryClient = createTestQueryClient(),
    ...renderOptions
  } = options ?? {};

  const authValue = {
    user,
    token,
    login: () => undefined,
    logout: () => undefined,
    isAuthenticated: Boolean(user && token),
    isLoading: false,
  };

  function Wrapper({ children }: PropsWithChildren) {
    return (
      <MemoryRouter initialEntries={[route]}>
        <ThemeProvider theme={theme}>
          <QueryClientProvider client={queryClient}>
            <AuthContext.Provider value={authValue}>
              <CssBaseline />
              {children}
            </AuthContext.Provider>
          </QueryClientProvider>
        </ThemeProvider>
      </MemoryRouter>
    );
  }

  return render(ui, { wrapper: Wrapper, ...renderOptions });
}

import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { clearAccessToken, setAccessToken } from "../../api/authToken";
import { toApiError, type ApiError } from "../../api/apiError";
import { getCurrentUser, login as loginRequest } from "./authApi";
import type { CurrentUserResponse, LoginRequest } from "./authTypes";
import { useAuth } from "./useAuth";

type LoginResult = {
  token: string;
  user: CurrentUserResponse;
};

function mapLoginError(error: unknown): ApiError {
  const apiError = toApiError(error);

  if (apiError.status === 401) {
    return {
      ...apiError,
      message: "Invalid email or password.",
    };
  }

  if (!apiError.status) {
    return {
      ...apiError,
      message: "Unable to connect to server.",
    };
  }

  return apiError;
}

export function useLogin() {
  const navigate = useNavigate();
  const { login } = useAuth();

  return useMutation<LoginResult, ApiError, LoginRequest>({
    mutationFn: async (request) => {
      const response = await loginRequest(request);
      setAccessToken(response.token);

      try {
        const currentUser = await getCurrentUser();
        return {
          token: response.token,
          user: currentUser,
        };
      } catch (error) {
        clearAccessToken();
        throw mapLoginError(error);
      }
    },
    onSuccess: (result) => {
      login(result.token, result.user);
      navigate("/dashboard", { replace: true });
    },
    onError: () => {
      clearAccessToken();
    },
  });
}

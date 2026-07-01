import { isAxiosError } from "axios";

export type ApiError = {
  message: string;
  status?: number;
  details?: unknown;
};

export function toApiError(error: unknown): ApiError {
  if (isAxiosError(error)) {
    if (error.response) {
      return {
        message:
          typeof error.response.data === "string"
            ? error.response.data
            : "An API error occurred.",
        status: error.response.status,
        details: error.response.data,
      };
    }

    if (error.request) {
      return {
        message: "Unable to reach the API. Please check the backend is running.",
      };
    }
  }

  return {
    message: "Unexpected error occurred.",
  };
}

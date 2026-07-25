import { isAxiosError } from "axios";

type ProblemDetails = {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
};

export type ApiError = {
  message: string;
  status?: number;
  details?: unknown;
  validationErrors?: string[];
};

function toValidationErrors(payload: unknown): string[] | undefined {
  if (!payload || typeof payload !== "object") {
    return undefined;
  }

  const errors = (payload as ProblemDetails).errors;
  if (!errors || typeof errors !== "object") {
    return undefined;
  }

  const messages = Object.values(errors)
    .flatMap((entry) => (Array.isArray(entry) ? entry : []))
    .map((entry) => String(entry).trim())
    .filter((entry) => entry.length > 0);

  return messages.length > 0 ? messages : undefined;
}

function toProblemMessage(payload: unknown): string | null {
  if (typeof payload === "string") {
    const text = payload.trim();
    return text.length > 0 ? text : null;
  }

  if (!payload || typeof payload !== "object") {
    return null;
  }

  const problem = payload as ProblemDetails;

  if (typeof problem.detail === "string" && problem.detail.trim().length > 0) {
    return problem.detail.trim();
  }

  if (typeof problem.title === "string" && problem.title.trim().length > 0) {
    return problem.title.trim();
  }

  const validationErrors = toValidationErrors(payload);
  if (validationErrors && validationErrors.length > 0) {
    return validationErrors[0];
  }

  return null;
}

export function toApiError(error: unknown): ApiError {
  if (isAxiosError(error)) {
    if (error.response) {
      const validationErrors = toValidationErrors(error.response.data);

      return {
        message: toProblemMessage(error.response.data) ?? "An API error occurred.",
        status: error.response.status,
        details: error.response.data,
        validationErrors,
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

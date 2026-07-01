import { get, post } from "../../api/request";
import type {
  AuthResponse,
  CurrentUserResponse,
  LoginRequest,
  RegisterRequest,
} from "./authTypes";

export function login(request: LoginRequest): Promise<AuthResponse> {
  return post<AuthResponse, LoginRequest>("/auth/login", request);
}

export function register(request: RegisterRequest): Promise<AuthResponse> {
  return post<AuthResponse, RegisterRequest>("/auth/register", request);
}

export function getCurrentUser(): Promise<CurrentUserResponse> {
  return get<CurrentUserResponse>("/auth/me");
}

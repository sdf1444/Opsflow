export type UserRole = "Employee" | "Manager" | "Admin";

export type LoginRequest = {
  email: string;
  password: string;
};

export type RegisterRequest = {
  name: string;
  email: string;
  password: string;
  role: UserRole;
};

export type AuthResponse = {
  token: string;
  email: string;
  name: string;
  role: UserRole;
};

export type CurrentUserResponse = {
  id: string;
  email: string;
  name: string;
  role: UserRole;
};

import { api } from "./client";

export async function get<T>(url: string): Promise<T> {
  const response = await api.get<T>(url);
  return response.data;
}

export async function post<TResponse, TBody = unknown>(
  url: string,
  body?: TBody,
): Promise<TResponse> {
  const response = await api.post<TResponse>(url, body);
  return response.data;
}

export async function put<TResponse, TBody = unknown>(
  url: string,
  body?: TBody,
): Promise<TResponse> {
  const response = await api.put<TResponse>(url, body);
  return response.data;
}

export async function remove<TResponse>(url: string): Promise<TResponse> {
  const response = await api.delete<TResponse>(url);
  return response.data;
}

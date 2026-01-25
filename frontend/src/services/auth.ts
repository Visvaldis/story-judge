import api from './api';
import type { User, LoginResponse } from '../types';

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export const authService = {
  async register(data: RegisterRequest): Promise<LoginResponse> {
    const response = await api.post<LoginResponse>('/auth/register', data);
    return response.data;
  },

  async login(data: LoginRequest): Promise<LoginResponse> {
    const response = await api.post<LoginResponse>('/auth/login', data);
    return response.data;
  },

  async getCurrentUser(): Promise<User> {
    const response = await api.get<User>('/auth/me');
    return response.data;
  },

  getGoogleLoginUrl(): string {
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
    return `${baseUrl}/auth/login/google`;
  },

  getLinkedInLoginUrl(): string {
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
    return `${baseUrl}/auth/login/linkedin`;
  },

  setToken(token: string): void {
    localStorage.setItem('token', token);
  },

  getToken(): string | null {
    return localStorage.getItem('token');
  },

  removeToken(): void {
    localStorage.removeItem('token');
  },

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
};

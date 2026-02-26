// services/authService.ts
import apiClient from '@/libs/apiClient';
import { RegisterRequest, LoginRequest, UserResponse, LoginResponse } from '../types/auth.types';

const authService = {
  register: async (data: RegisterRequest): Promise<UserResponse> => {
    const response = await apiClient.post<UserResponse>('/auth/register', data);
    return response.data;
  },

  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/auth/login', data);
    localStorage.setItem('token', response.data.token);
    return response.data;
  },

  logout: () => {
    localStorage.removeItem('token');
  },

  getUser: async (id: string): Promise<UserResponse> => {
    const response = await apiClient.get<UserResponse>(`/auth/user/${id}`);
    return response.data;
  },

  getCurrentUserId: (): string | null => {
    const token = localStorage.getItem('token');
    if (!token) return null;
    return null; // optionally decode JWT to get userId
  }
};

export default authService;
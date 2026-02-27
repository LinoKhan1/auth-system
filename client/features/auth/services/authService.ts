// services/authService.ts
import apiClient from '@/libs/apiClient';
import { RegisterRequest, LoginRequest, UserResponse, LoginResponse } from '../types/auth.types';

const authService = {
  register: async (data: RegisterRequest): Promise<UserResponse> => {
    const response = await apiClient.post<UserResponse>('/Auth/register', data);
    return response.data;
  },

  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/Auth/login', data);
    localStorage.setItem('token', response.data.token); // store token
    return response.data;
  },

  logout: () => {
    localStorage.removeItem('token');
  },

  // Only one method to get user by ID
  getUser: async (id: string): Promise<UserResponse> => {
    const token = localStorage.getItem('token');
    if (!token) throw new Error('No token found');

    const response = await apiClient.get<UserResponse>(`/Auth/user/${id}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    return response.data;
  }
};

export default authService;
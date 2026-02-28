// services/authService.ts
import apiClient from '@/libs/apiClient';
import { RegisterRequest, LoginRequest, UserResponse, LoginResponse } from '../types/auth.types';

// Service for handling authentication-related API calls
const authService = {
  // Register a new user
  register: async (data: RegisterRequest): Promise<UserResponse> => {
    try {
      const response = await apiClient.post<UserResponse>('/Auth/register', data);
      return response.data;
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Register failed';
      console.error('Register failed:', message);
      ;
      throw new Error(message);
    }
  },

  // Login an existing user (server sets session cookie)
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    try {
      const response = await apiClient.post<LoginResponse>('/Auth/login', data);
      return response.data;
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Login failed';
      console.error('Login failed:', message);
      throw new Error(message);
    }
  },


  // Logout the user (server clears session cookie)
  logout: async (): Promise<void> => {
    try {
      await apiClient.post('/Auth/logout');
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Logout failed';
      console.error('Logout failed:', message);
    }
  },


  // Get user details by ID (server uses session cookie)
  getUser: async (): Promise<UserResponse> => {
    try {
      const response = await apiClient.get<UserResponse>("/Auth/user/me");
      return response.data;
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to fetch user';
      console.error('GetUser failed:', message);
      throw new Error(message);
    }
  },
};


export default authService;
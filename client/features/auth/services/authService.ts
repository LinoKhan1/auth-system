import apiClient from '@/libs/apiClient';
import { RegisterRequest, LoginRequest, UserResponse, LoginResponse } from '../types/auth.types';

// Service for handling authentication-related API calls
const authService = {
  // Register a new user
  register: async (data: RegisterRequest): Promise<UserResponse> => {
    try {
      const response = await apiClient.post<UserResponse>('/Auth/register', data);
      return response.data;
    } catch (err: any) {
      console.error('Register failed', err);
      throw new Error(err?.response?.data?.message || err.message || 'Register failed');
    }
  },

  // Login an existing user
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    try {
      const response = await apiClient.post<LoginResponse>('/Auth/login', data);
      localStorage.setItem('token', response.data.token); // store token
      return response.data;
    } catch (err: any) {
      console.error('Login failed', err);
      throw new Error(err?.response?.data?.message || err.message || 'Login failed');
    }
  },

  // Logout the user by removing the token
  logout: () => {
    try {
      localStorage.removeItem('token');
    } catch (err) {
      console.error('Logout failed', err);
    }
  },

  // Get user details by ID
  getUser: async (id: string): Promise<UserResponse> => {
    try {
      const token = localStorage.getItem('token');
      if (!token) throw new Error('No token found');

      const response = await apiClient.get<UserResponse>(`/Auth/user/${id}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      return response.data;
    } catch (err: any) {
      console.error('GetUser failed', err);
      throw new Error(err?.response?.data?.message || err.message || 'GetUser failed');
    }
  }
};

export default authService;
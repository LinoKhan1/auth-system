// hooks/useAuth.ts
"use client";
import { useState } from 'react';
import authService from '../services/authService';
import { RegisterRequest, LoginRequest, UserResponse, LoginResponse } from '../types/auth.types';

export const useAuth = () => {
  const [user, setUser] = useState<UserResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getToken = (): string | null => {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('token');
  };

  const register = async (data: RegisterRequest) => {
    setLoading(true);
    setError(null);
    try {
      const newUser = await authService.register(data);
      setLoading(false);
      return newUser;
    } catch (err: any) {
      setError(err?.message || 'Unknown error');
      setLoading(false);
      throw err;
    }
  };

  const login = async (data: LoginRequest) => {
    setLoading(true);
    setError(null);
    try {
      const loginData: LoginResponse = await authService.login(data);
      setLoading(false);
      return loginData;
    } catch (err: any) {
      setError(err?.message || 'Unknown error');
      setLoading(false);
      throw err;
    }
  };

  const logout = () => {
    authService.logout();
    setUser(null);
  };

  const fetchUser = async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const currentUser = await authService.getUser(id);
      setUser(currentUser);
      setLoading(false);
      return currentUser;
    } catch (err: any) {
      setError(err?.message || 'Unknown error');
      setLoading(false);
      throw err;
    }
  };

  return { user, loading, error, register, login, logout, fetchUser, getToken };
};
"use client";

import { useState } from 'react';
import authService from '../services/authService';
import { RegisterRequest, LoginRequest, UserResponse, LoginResponse } from '../types/auth.types';

export const useAuth = () => {
  const [user, setUser] = useState<UserResponse | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Save JWT token in localStorage
  const saveToken = (token: string) => {
    if (typeof window !== 'undefined') localStorage.setItem('token', token);
  };

  const getToken = (): string | null => {
    if (typeof window !== 'undefined') return localStorage.getItem('token');
    return null;
  };

  const register = async (data: RegisterRequest): Promise<UserResponse> => {
    setLoading(true);
    setError(null);
    try {
      const newUser = await authService.register(data);
      setUser(newUser);
      setLoading(false);
      return newUser;
    } catch (err: any) {
      const message = err?.response?.data?.message || err.message || 'Unknown error';
      setError(message);
      setLoading(false);
      throw new Error(message);
    }
  };

  const login = async (data: LoginRequest): Promise<LoginResponse> => {
    setLoading(true);
    setError(null);
    try {
      const loginData = await authService.login(data); // returns { user, token }
      setUser(loginData.user);
      saveToken(loginData.token); // store token locally
      setLoading(false);
      return loginData;
    } catch (err: any) {
      const message = err?.response?.data?.message || err.message || 'Unknown error';
      setError(message);
      setLoading(false);
      throw new Error(message);
    }
  };

  const logout = (): void => {
    if (typeof window !== 'undefined') localStorage.removeItem('token');
    setUser(null);
  };

  const fetchUser = async (id: string): Promise<UserResponse> => {
    setLoading(true);
    setError(null);
    try {
      // Only pass ID, authService internally handles token if needed
      const currentUser = await authService.getUser(id);
      setUser(currentUser);
      setLoading(false);
      return currentUser;
    } catch (err: any) {
      const message = err?.response?.data?.message || err.message || 'Unknown error';
      setError(message);
      setLoading(false);
      throw new Error(message);
    }
  };

  return { user, loading, error, register, login, logout, fetchUser, getToken };
};
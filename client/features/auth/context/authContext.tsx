"use client";

import { createContext, useState, ReactNode, useCallback } from "react";
import authService from "../services/authService";
import {
  RegisterRequest,
  LoginRequest,
  UserResponse,
  LoginResponse,
} from "../types/auth.types";

// ------------------------
// Context type
// ------------------------
export interface AuthContextType {
  user: UserResponse | null;
  loading: boolean;
  error: string | null;
  register: (data: RegisterRequest) => Promise<UserResponse>;
  login: (data: LoginRequest) => Promise<LoginResponse>;
  logout: () => Promise<void>;
  fetchUser: () => Promise<UserResponse | null>;
}

// ------------------------
// Create context
// ------------------------
export const AuthContext = createContext<AuthContextType | undefined>(undefined);

// ------------------------
// Provider
// ------------------------
interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<UserResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // ------------------------
  // REGISTER
  // ------------------------
  const register = useCallback(async (data: RegisterRequest) => {
    setLoading(true);
    setError(null);

    try {
      const newUser = await authService.register(data);
      setUser(newUser);
      return newUser;
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : "Unknown registration error";
      setError(message);
      throw new Error(message);
    } finally {
      setLoading(false);
    }
  }, []);

  // ------------------------
  // LOGIN
  // ------------------------
  const login = useCallback(async (data: LoginRequest) => {
    setLoading(true);
    setError(null);

    try {
      const loginData = await authService.login(data);
      return loginData;
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : "Unknown login error";
      setError(message);
      throw new Error(message);
    } finally {
      setLoading(false);
    }
  }, []);

  // ------------------------
  // LOGOUT
  // ------------------------
  const logout = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      await authService.logout();
      setUser(null);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : "Unknown logout error";
      setError(message);
      throw new Error(message);
    } finally {
      setLoading(false);
    }
  }, []);

  // ------------------------
  // FETCH CURRENT USER
  // ------------------------
  const fetchUser = useCallback(async (): Promise<UserResponse | null> => {
    setLoading(true);
    setError(null);

    try {
      const currentUser = await authService.getUser();
      setUser(currentUser);
      return currentUser;
    } catch (err: unknown) {
      setUser(null);
      const message = err instanceof Error ? err.message : "Unknown error fetching user";
      setError(message);
      return null; 
    } finally {
      setLoading(false);
    }
  }, []);

  return (
    <AuthContext.Provider
      value={{ user, loading, error, register, login, logout, fetchUser }}
    >
      {children}
    </AuthContext.Provider>
  );
};


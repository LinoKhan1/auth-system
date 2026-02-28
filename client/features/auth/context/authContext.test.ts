// features/auth/hooks/useAuth.test.ts
import { renderHook, act } from '@testing-library/react';
import React from 'react';
import { AuthProvider, AuthContext, AuthContextType } from './authContext';
import authService from '../services/authService';

// ------------------------
// Mock authService methods
// ------------------------
jest.mock('../services/authService');
const mockedAuthService = authService as jest.Mocked<typeof authService>;


// ------------------------
// Wrapper component for renderHook
// ------------------------
const wrapper = ({ children }: { children: React.ReactNode }) => 
  React.createElement(AuthProvider, null, children);

// ------------------------
// Tests
// ------------------------
describe('AuthContext', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorage.clear();
  });

  const mockUser = {
    id: '1',
    firstName: 'Test',
    lastName: 'User',
    email: 'test@example.com',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  };

  it('registers a user successfully', async () => {
    mockedAuthService.register.mockResolvedValue(mockUser);

    const { result } = renderHook(() => React.useContext(AuthContext) as AuthContextType, { wrapper });

    await act(async () => {
      const user = await result.current.register({ firstName: 'Test', lastName: 'User', email: 'test@example.com', password: 'pass' });
      expect(user).toEqual(mockUser);
    });

    expect(result.current.user).toEqual(mockUser);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('handles register error', async () => {
    const error = new Error('Email exists');
    mockedAuthService.register.mockRejectedValue(error);

    const { result } = renderHook(() => React.useContext(AuthContext) as AuthContextType, { wrapper });

    await act(async () => {
      await expect(result.current.register({ firstName: 'A', lastName: 'B', email: 'x@y.com', password: 'p' }))
        .rejects.toThrow('Email exists');
    });

    expect(result.current.user).toBeNull();
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBe('Email exists');
  });

  it('logs in a user successfully', async () => {
    const loginResponse = { token: 'mock-token' };
    mockedAuthService.login.mockResolvedValue(loginResponse);

    const { result } = renderHook(() => React.useContext(AuthContext) as AuthContextType, { wrapper });

    await act(async () => {
      const data = await result.current.login({ email: 'test@example.com', password: 'pass' });
      expect(data).toEqual(loginResponse);
    });

    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('handles login error', async () => {
    const error = new Error('Invalid credentials');
    mockedAuthService.login.mockRejectedValue(error);

    const { result } = renderHook(() => React.useContext(AuthContext) as AuthContextType, { wrapper });

    await act(async () => {
      await expect(result.current.login({ email: 'x@y.com', password: 'p' }))
        .rejects.toThrow('Invalid credentials');
    });

    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBe('Invalid credentials');
  });

  it('logs out correctly', async () => {
    mockedAuthService.logout.mockResolvedValue();

    const { result } = renderHook(() => React.useContext(AuthContext) as AuthContextType, { wrapper });

    await act(async () => {
      await result.current.logout();
    });

    expect(result.current.user).toBeNull();
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('fetches user successfully', async () => {
    mockedAuthService.getUser.mockResolvedValue(mockUser);

    const { result } = renderHook(() => React.useContext(AuthContext) as AuthContextType, { wrapper });

    await act(async () => {
      const user = await result.current.fetchUser();
      expect(user).toEqual(mockUser);
    });

    expect(result.current.user).toEqual(mockUser);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('handles fetchUser error', async () => {
    const error = new Error('User not found');
    mockedAuthService.getUser.mockRejectedValue(error);

    const { result } = renderHook(() => React.useContext(AuthContext) as AuthContextType, { wrapper });

    await act(async () => {
      const user = await result.current.fetchUser();
      expect(user).toBeNull();
    });

    expect(result.current.user).toBeNull();
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBe('User not found');
  });
});
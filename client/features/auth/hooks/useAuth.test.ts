// features/auth/hooks/useAuth.test.ts
import { renderHook, act } from '@testing-library/react';
import { useAuth } from './useAuth';
import authService from '../services/authService';

// ------------------------
// Mock authService methods
// ------------------------
jest.mock('../services/authService');
const mockedAuthService = authService as jest.Mocked<typeof authService>;

// ------------------------
// Mock localStorage
// ------------------------
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: (key: string) => store[key] || null,
    setItem: (key: string, value: string) => { store[key] = value.toString(); },
    removeItem: (key: string) => { delete store[key]; },
    clear: () => { store = {}; }
  };
})();

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock
});

// ------------------------
// Tests
// ------------------------
describe('useAuth hook', () => {

  beforeEach(() => {
    jest.clearAllMocks();
    localStorage.clear();
  });

  it('registers a user successfully', async () => {
    const mockUser = { id: '1', firstName: 'Test', lastName: 'User', email: 'test@example.com', createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() };
    mockedAuthService.register.mockResolvedValue(mockUser);

    const { result } = renderHook(() => useAuth());

    await act(async () => {
      const user = await result.current.register({ firstName: 'Test', lastName: 'User', email: 'test@example.com', password: 'pass' });
      expect(user).toEqual(mockUser);
    });

    expect(result.current.user).toBeNull(); // user state is only set on fetchUser
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('handles register error', async () => {
    const error = new Error('Email exists');
    mockedAuthService.register.mockRejectedValue(error);

    const { result } = renderHook(() => useAuth());

    await act(async () => {
      await expect(result.current.register({ firstName: 'A', lastName: 'B', email: 'x@y.com', password: 'p' }))
        .rejects.toThrow('Email exists');
    });

    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBe('Email exists');
  });

  it('logs in a user successfully', async () => {
    const loginResponse = { token: 'mock-token' };
    mockedAuthService.login.mockResolvedValue(loginResponse);

    const { result } = renderHook(() => useAuth());

    await act(async () => {
      const data = await result.current.login({ email: 'test@example.com', password: 'pass' });
      expect(data).toEqual(loginResponse);
    });

    expect(localStorage.getItem('token')).toBe('mock-token'); 
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('handles login error', async () => {
    const error = new Error('Invalid credentials');
    mockedAuthService.login.mockRejectedValue(error);

    const { result } = renderHook(() => useAuth());

    await act(async () => {
      await expect(result.current.login({ email: 'x@y.com', password: 'p' }))
        .rejects.toThrow('Invalid credentials');
    });

    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBe('Invalid credentials');
  });

  it('logs out correctly', () => {
    localStorage.setItem('token', 'mock-token');
    const { result } = renderHook(() => useAuth());

    act(() => result.current.logout());

    expect(localStorage.getItem('token')).toBeNull(); 
    expect(result.current.user).toBeNull();
  });

  it('fetches user successfully', async () => {
    const mockUser = { id: '1', firstName: 'Test', lastName: 'User', email: 'test@example.com', createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() };
    mockedAuthService.getUser.mockResolvedValue(mockUser);
    localStorage.setItem('token', 'mock-token');

    const { result } = renderHook(() => useAuth());

    await act(async () => {
      const user = await result.current.fetchUser('1');
      expect(user).toEqual(mockUser);
    });

    expect(result.current.user).toEqual(mockUser);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('handles fetchUser error', async () => {
    const error = new Error('User not found');
    mockedAuthService.getUser.mockRejectedValue(error);
    localStorage.setItem('token', 'mock-token');

    const { result } = renderHook(() => useAuth());

    await act(async () => {
      await expect(result.current.fetchUser('1')).rejects.toThrow('User not found');
    });

    expect(result.current.user).toBeNull();
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBe('User not found');
  });

  it('getToken returns null when no token', () => {
    const { result } = renderHook(() => useAuth());
    expect(result.current.getToken()).toBeNull();
  });

  it('getToken returns the stored token', () => {
    localStorage.setItem('token', 'mock-token');
    const { result } = renderHook(() => useAuth());
    expect(result.current.getToken()).toBe('mock-token');
  });

});
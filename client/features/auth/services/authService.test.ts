import authService from './authService';
import apiClient from '.../../../libs/apiClient';
import AxiosMockAdapter from 'axios-mock-adapter';

const mock = new AxiosMockAdapter(apiClient);

describe('authService', () => {
  beforeEach(() => {
    mock.reset();
    localStorage.clear();
  });

  const testUser = {
    id: 1,
    firstName: 'Test',
    lastName: 'User',
    email: 'test@example.com',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  };

  it('registers a user successfully', async () => {
    const payload = { firstName: 'Test', lastName: 'User', email: 'test@example.com', password: 'password' };
    mock.onPost('/Auth/register').reply(200, testUser);

    const result = await authService.register(payload);
    expect(result).toEqual(testUser);
  });

  it('throws error when register fails', async () => {
    mock.onPost('/Auth/register').reply(400, { message: 'Email already used' });

    await expect(authService.register({ firstName:'a', lastName:'b', email:'x@y.com', password:'p'}))
      .rejects.toThrow('Email already used');
  });

  it('logs in a user and stores token', async () => {
    const token = 'mock-token';
    mock.onPost('/Auth/login').reply(200, { token });

    const result = await authService.login({ email: 'test@example.com', password: 'password' });
    expect(result.token).toBe(token);
    expect(localStorage.getItem('token')).toBe(token);
  });

  it('throws error when login fails', async () => {
    mock.onPost('/Auth/login').reply(401, { message: 'Invalid credentials' });

    await expect(authService.login({ email:'x@y.com', password:'p'}))
      .rejects.toThrow('Invalid credentials');
  });

  it('removes token on logout', () => {
    localStorage.setItem('token', 'mock-token');
    authService.logout();
    expect(localStorage.getItem('token')).toBeNull();
  });

  it('gets user with valid token', async () => {
    const token = 'mock-token';
    localStorage.setItem('token', token);

    mock.onGet('/Auth/user/1').reply(config => {
      expect(config.headers?.Authorization).toBe(`Bearer ${token}`);
      return [200, testUser];
    });

    const user = await authService.getUser('1');
    expect(user).toEqual(testUser);
  });

  it('throws error if no token is present', async () => {
    await expect(authService.getUser('1')).rejects.toThrow('No token found');
  });
});
import authService from './authService';
import apiClient from '.../../../libs/apiClient';
import AxiosMockAdapter from 'axios-mock-adapter';

const mock = new AxiosMockAdapter(apiClient);

describe('authService (cookie-based auth)', () => {
  beforeEach(() => {
    mock.reset();
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
    // Mock a 400 Bad Request response
    mock.onPost('/Auth/register').reply(400);

    await expect(
      authService.register({ firstName: 'a', lastName: 'b', email: 'x@y.com', password: 'p' })
    ).rejects.toThrow("Request failed with status code 400");
  });

  it('logs in a user successfully', async () => {
    const token = 'mock-token';
    mock.onPost('/Auth/login').reply(200, { token });

    const result = await authService.login({ email: 'test@example.com', password: 'password' });
    expect(result.token).toBe(token);

    // No localStorage check anymore; cookie handled by browser/axios
  });

  it('throws error when login fails', async () => {
    mock.onPost('/Auth/login').reply(401, { message: 'Request failed with status code 401' });

    await expect(authService.login({ email: 'x@y.com', password: 'p' }))
      .rejects.toThrow('Request failed with status code 401');
  });

  it('calls logout endpoint successfully', async () => {
    mock.onPost('/Auth/logout').reply(200);

    await expect(authService.logout()).resolves.toBeUndefined();
  });

  it('gets user successfully via cookie', async () => {
    mock.onGet('/Auth/user/me').reply(config => {
      // Axios automatically sends cookies with withCredentials
      return [200, testUser];
    });

    const user = await authService.getUser();
    expect(user).toEqual(testUser);
  });

  it('throws error if getUser fails', async () => {
    mock.onGet('/Auth/user/me').reply(401, { message: 'Request failed with status code 401' });

    await expect(authService.getUser()).rejects.toThrow('Request failed with status code 401');
  });
});
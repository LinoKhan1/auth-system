// middleware/authMiddleware.ts
import { NextRouter } from 'next/router';

export const requireAuth = (router: NextRouter) : boolean => {
  const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;

  if (!token) {
    router.push('/login'); 
    return false;
  }

  return true;
};
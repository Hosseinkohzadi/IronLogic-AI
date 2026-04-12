import { HttpInterceptorFn } from '@angular/common/http';

const TOKEN_STORAGE_KEY = 'ironlogic.auth.token';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem(TOKEN_STORAGE_KEY);

  if (!token) {
    return next(req);
  }

  // Attach bearer token for API requests while keeping static asset requests untouched.
  const isApiRequest = req.url.includes('/api/') || req.url.startsWith('http');
  if (!isApiRequest) {
    return next(req);
  }

  const normalizedUrl = req.url.toLowerCase();
  const isAuthRequest =
    normalizedUrl.includes('/auth/login') || normalizedUrl.includes('/auth/register');
  if (isAuthRequest) {
    return next(req);
  }

  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });

  return next(authReq);
};

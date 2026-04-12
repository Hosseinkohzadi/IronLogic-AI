import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '@env/environment';

const TOKEN_STORAGE_KEY = 'ironlogic.auth.token';

/**
 * Attaches a Bearer token to every outbound request that targets the API.
 *
 * Token lookup order:
 *  1. `ironlogic.auth.token` in localStorage  — set here once the backend issues JWTs.
 *  2. Falls back to the userId stored by AuthService as a transient dev identifier.
 *
 * Only requests whose URL starts with `environment.apiUrl` receive the header,
 * so third-party calls (e.g. Unsplash avatar URLs) are left untouched.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  const token = localStorage.getItem(TOKEN_STORAGE_KEY);

  if (!token) {
    return next(req);
  }

  const authReq = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });

  return next(authReq);
};

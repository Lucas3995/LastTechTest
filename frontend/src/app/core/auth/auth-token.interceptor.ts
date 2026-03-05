import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { AuthService } from './auth.service';
import { API_BASE_URL } from '../api-base-url';

/**
 * Adds Authorization: Bearer <accessToken> to outgoing requests whose URL
 * starts with the API base URL when the current session has an access token.
 * Used to fix 401 Unauthorized on "Minhas solicitações de antecipação".
 */
export const authTokenInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next) => {
  const apiBase = inject(API_BASE_URL);
  const auth = inject(AuthService);

  if (!req.url.startsWith(apiBase)) {
    return next(req);
  }

  const session = auth.session();
  const token = session?.accessToken?.trim();
  if (!token) {
    return next(req);
  }

  const cloned = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });
  return next(cloned);
};

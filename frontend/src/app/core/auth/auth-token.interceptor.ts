import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { TokenRefreshService } from './token-refresh.service';
import { API_BASE_URL } from '../api-base-url';

/** URLs que não devem disparar retry com refresh (evitar loop). */
const SKIP_REFRESH_URL_SUFFIXES = ['/auth/refresh', '/auth/login'];

/**
 * Adds Authorization: Bearer <accessToken> to outgoing requests whose URL
 * starts with the API base URL. Only 401 is treated as auth error (token/session invalid):
 * tries refresh then retries; on refresh failure or no refresh token, logs out and
 * redirects to login with returnUrl. If the retry returns 401, also logs out and redirects.
 * 403 is not treated as auth and always propagates to the caller (API contract: business rules use 400).
 */
export const authTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const apiBase = inject(API_BASE_URL);
  const auth = inject(AuthService);
  const refreshService = inject(TokenRefreshService);

  if (!req.url.startsWith(apiBase)) {
    return next(req);
  }

  const session = auth.session();
  const token = session?.accessToken?.trim();
  const cloned = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(cloned).pipe(
    catchError((err) => {
      if (!(err instanceof HttpErrorResponse) || err.status !== 401) {
        return throwError(() => err);
      }

      const isRefreshOrLogin = SKIP_REFRESH_URL_SUFFIXES.some((s) => req.url.includes(s));
      if (isRefreshOrLogin || !auth.session()?.refreshToken?.trim()) {
        refreshService.logoutAndRedirect();
        return throwError(() => err);
      }

      return refreshService.refreshOrLogout().pipe(
        switchMap((newSession) => {
          const retryReq = req.clone({
            setHeaders: { Authorization: `Bearer ${newSession.accessToken}` },
          });
          return next(retryReq).pipe(
            catchError((retryErr) => {
              if (retryErr instanceof HttpErrorResponse && retryErr.status === 401) {
                refreshService.logoutAndRedirect();
              }
              return throwError(() => retryErr);
            }),
          );
        }),
      );
    }),
  );
};

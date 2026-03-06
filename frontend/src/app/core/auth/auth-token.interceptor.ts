import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, finalize, Observable, shareReplay, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { AuthSession } from './auth.service';
import { API_BASE_URL } from '../api-base-url';

/** URLs que não devem disparar retry com refresh (evitar loop). */
const SKIP_REFRESH_URL_SUFFIXES = ['/auth/refresh', '/auth/login'];

/** Uma única renovação em curso; reutilizada por múltiplos 401 simultâneos. */
let refreshInFlight: Observable<AuthSession> | null = null;

/**
 * Adds Authorization: Bearer <accessToken> to outgoing requests whose URL
 * starts with the API base URL. Only 401 is treated as auth error (token/session invalid):
 * tries refresh then retries; on refresh failure or no refresh token, logs out and
 * redirects to login with returnUrl. If the retry returns 401, also logs out and redirects.
 * 403 is not treated as auth and always propagates to the caller (API contract: business rules use 400).
 */
export const authTokenInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next) => {
  const apiBase = inject(API_BASE_URL);
  const auth = inject(AuthService);
  const router = inject(Router);

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
      const isAuthError =
        err instanceof HttpErrorResponse && err.status === 401;
      if (!isAuthError) {
        return throwError(() => err);
      }
      const isRefreshOrLogin = SKIP_REFRESH_URL_SUFFIXES.some((s) => req.url.includes(s));
      if (isRefreshOrLogin) {
        auth.logout();
        const returnUrl = encodeURIComponent(typeof window !== 'undefined' ? window.location.pathname + window.location.search : '/');
        router.navigateByUrl(`/?returnUrl=${returnUrl}`);
        return throwError(() => err);
      }
      const refreshToken = auth.session()?.refreshToken?.trim();
      if (!refreshToken) {
        auth.logout();
        const returnUrl = encodeURIComponent(typeof window !== 'undefined' ? window.location.pathname + window.location.search : '/');
        router.navigateByUrl(`/?returnUrl=${returnUrl}`);
        return throwError(() => err);
      }
      if (!refreshInFlight) {
        refreshInFlight = auth.refresh().pipe(
          shareReplay({ bufferSize: 1, refCount: true }),
          catchError((e) => {
            refreshInFlight = null;
            auth.logout();
            const returnUrl = encodeURIComponent(typeof window !== 'undefined' ? window.location.pathname + window.location.search : '/');
            router.navigateByUrl(`/?returnUrl=${returnUrl}`);
            return throwError(() => e);
          }),
          finalize(() => {
            refreshInFlight = null;
          }),
        );
      }
      return refreshInFlight.pipe(
        switchMap((newSession) => {
          const retryReq = req.clone({
            setHeaders: { Authorization: `Bearer ${newSession.accessToken}` },
          });
          return next(retryReq).pipe(
            catchError((retryErr) => {
              const isRetryAuthError =
                retryErr instanceof HttpErrorResponse && retryErr.status === 401;
              if (isRetryAuthError) {
                auth.logout();
                const returnUrl = encodeURIComponent(
                  typeof window !== 'undefined' ? window.location.pathname + window.location.search : '/'
                );
                router.navigateByUrl(`/?returnUrl=${returnUrl}`);
              }
              return throwError(() => retryErr);
            })
          );
        }),
      );
    }),
  );
};

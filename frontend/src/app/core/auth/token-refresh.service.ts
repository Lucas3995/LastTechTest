import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, finalize, shareReplay, throwError } from 'rxjs';

import { AuthService, AuthSession } from './auth.service';

/**
 * Manages token refresh flow: deduplicates concurrent refreshes,
 * handles logout + redirect on failure.
 */
@Injectable({ providedIn: 'root' })
export class TokenRefreshService {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  private refreshInFlight: Observable<AuthSession> | null = null;

  /**
   * Logs out the current user and navigates to login with a returnUrl.
   */
  logoutAndRedirect(): void {
    this.auth.logout();
    const returnUrl = encodeURIComponent(
      typeof window !== 'undefined'
        ? window.location.pathname + window.location.search
        : '/',
    );
    this.router.navigateByUrl(`/?returnUrl=${returnUrl}`);
  }

  /**
   * Returns a shared Observable that performs the token refresh.
   * Multiple concurrent callers share the same in-flight request.
   * On failure, logs out and redirects.
   */
  refreshOrLogout(): Observable<AuthSession> {
    if (!this.refreshInFlight) {
      this.refreshInFlight = this.auth.refresh().pipe(
        shareReplay({ bufferSize: 1, refCount: true }),
        catchError((e) => {
          this.refreshInFlight = null;
          this.logoutAndRedirect();
          return throwError(() => e);
        }),
        finalize(() => {
          this.refreshInFlight = null;
        }),
      );
    }
    return this.refreshInFlight;
  }
}

import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from './auth.service';

/** Default home path for authenticated users (home da área). Can be extended per-role later. */
const DEFAULT_LOGGED_HOME_PATH = '/home';

/**
 * Guard for the login route (e.g. path '').
 * If the user is authenticated, redirects to the home of their area; otherwise allows access (login page).
 */
export const redirectIfAuthenticatedGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return router.createUrlTree([DEFAULT_LOGGED_HOME_PATH]);
  }

  return true;
};

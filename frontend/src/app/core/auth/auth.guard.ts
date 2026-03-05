import { inject } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
} from '@angular/router';

import { AuthService } from './auth.service';

export const AuthGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isAuthenticated = authService.isAuthenticated();

  if (!isAuthenticated) {
    const returnUrl = state.url || '/home';
    return router.createUrlTree(['/'], {
      queryParams: { returnUrl },
    });
  }

  const requiredRoles = (route.data?.['requiredRoles'] as string[] | undefined) ?? (route.data?.['requiredRole'] as string | undefined);
  const allowedRoles: string[] = Array.isArray(requiredRoles)
    ? requiredRoles
    : requiredRoles
      ? [requiredRoles]
      : [];
  const user = authService.currentUser();

  if (allowedRoles.length > 0 && user && !allowedRoles.includes(user.role)) {
    return router.createUrlTree(['/']);
  }

  return true;
};


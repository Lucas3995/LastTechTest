import { Routes } from '@angular/router';
import { AuthGuard, redirectIfAuthenticatedGuard } from './core';

export const routes: Routes = [
  {
    path: '',
    canActivate: [redirectIfAuthenticatedGuard],
    loadComponent: () =>
      import('./features/auth/pages/login/login-page.component').then((m) => m.LoginPageComponent),
  },
  {
    path: 'home',
    canActivate: [AuthGuard],
    loadComponent: () =>
      import('./features/home/pages/home/home.component').then((m) => m.HomePageComponent),
  },
  {
    path: 'anticipation/my-requests',
    canActivate: [AuthGuard],
    data: { requiredRoles: ['Creator', 'Admin'] },
    loadComponent: () =>
      import('./features/anticipation/pages/my-requests/anticipation-my-requests-page.component').then(
        (m) => m.AnticipationMyRequestsPageComponent,
      ),
  },
  {
    path: 'login',
    redirectTo: '',
    pathMatch: 'full',
  },
];

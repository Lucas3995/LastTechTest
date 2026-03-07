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
        (m) => m.AnticipationMyRequestsPageComponent
      ),
  },
  {
    path: 'anticipation/my-requests/new',
    canActivate: [AuthGuard],
    data: { requiredRoles: ['Creator', 'Admin'] },
    loadComponent: () =>
      import('./features/anticipation/pages/new-request/anticipation-new-request-page.component').then(
        (m) => m.AnticipationNewRequestPageComponent
      ),
  },
  {
    path: 'anticipation/list',
    canActivate: [AuthGuard],
    data: { requiredRoles: ['Admin', 'Analista'] },
    loadComponent: () =>
      import('./features/anticipation/pages/admin-list/anticipation-admin-requests-page.component').then(
        (m) => m.AnticipationAdminRequestsPageComponent
      ),
  },
  {
    path: 'anticipation/simulation',
    canActivate: [AuthGuard],
    data: { requiredRoles: ['Creator', 'Admin', 'Analista'] },
    loadComponent: () =>
      import('./features/anticipation/pages/simulation/anticipation-simulation-page.component').then(
        (m) => m.AnticipationSimulationPageComponent
      ),
  },
  {
    path: 'auth/change-password',
    canActivate: [AuthGuard],
    data: { requiredRoles: ['Creator', 'Analista'] },
    loadComponent: () =>
      import('./features/auth/pages/change-password/change-password-page.component').then(
        (m) => m.ChangePasswordPageComponent
      ),
  },
  {
    path: 'admin/users',
    canActivate: [AuthGuard],
    data: { requiredRoles: ['Admin'] },
    loadComponent: () =>
      import('./features/auth/pages/admin-users/admin-users-page.component').then(
        (m) => m.AdminUsersPageComponent
      ),
  },
  {
    path: 'login',
    redirectTo: '',
    pathMatch: 'full',
  },
];

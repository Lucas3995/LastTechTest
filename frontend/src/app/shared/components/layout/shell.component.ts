import { Component, computed, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NavigationEnd,
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet,
} from '@angular/router';
import { filter } from 'rxjs';

import { AuthService } from '../../../core';

/** Menu item visible only to given roles (área logada por role). */
export interface ShellNavItem {
  path: string;
  label: string;
  roles: string[];
}

const SHELL_NAV_ITEMS: ShellNavItem[] = [
  { path: '/home', label: 'Home', roles: ['Creator', 'Analista', 'Admin'] },
  { path: '/anticipation/my-requests', label: 'Minhas antecipações', roles: ['Creator', 'Admin'] },
  { path: '/anticipation/list', label: 'Lista global de solicitações', roles: ['Analista', 'Admin'] },
  { path: '/auth/change-password', label: 'Alterar senha', roles: ['Creator', 'Analista'] },
  { path: '/admin/users', label: 'Gestão de Usuários', roles: ['Admin'] },
];

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  readonly authService = inject(AuthService);

  readonly navItems = computed(() => {
    const user = this.authService.currentUser();
    if (!user) return [];
    return SHELL_NAV_ITEMS.filter((item) => item.roles.includes(user.role));
  });

  constructor() {
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        const main = document.getElementById('main-content');
        if (main) {
          main.focus();
        }
      });
  }

  onLogout(): void {
    this.authService.logout();
    const returnUrl =
      typeof window !== 'undefined'
        ? encodeURIComponent(window.location.pathname + window.location.search)
        : '';
    this.router.navigateByUrl(returnUrl ? `/?returnUrl=${returnUrl}` : '/');
  }
}

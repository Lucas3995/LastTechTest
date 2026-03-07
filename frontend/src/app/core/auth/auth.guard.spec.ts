import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';

import { AuthGuard, AuthService, AuthUser } from '../../core';

describe('AuthGuard', () => {
  const createUrlTree = vi.fn(() => ({} as unknown as UrlTree));
  const routerMock = { createUrlTree };

  const isAuthenticatedFn = vi.fn(() => false);
  const currentUserFn = vi.fn((): AuthUser | null => null);
  const authServiceMock = {
    isAuthenticated: isAuthenticatedFn,
    currentUser: currentUserFn,
  } as unknown as AuthService;

  beforeEach(() => {
    vi.clearAllMocks();
    isAuthenticatedFn.mockReturnValue(false);
    currentUserFn.mockReturnValue(null);
    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: routerMock },
        { provide: AuthService, useValue: authServiceMock },
      ],
    });
  });

  it('deve redirecionar para rota raiz (login) quando não autenticado', () => {
    TestBed.runInInjectionContext(() =>
      AuthGuard(
        { data: {} } as unknown as ActivatedRouteSnapshot,
        { url: '/anticipation/my-requests' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(createUrlTree).toHaveBeenCalledWith(
      ['/'],
      expect.objectContaining({ queryParams: { returnUrl: '/anticipation/my-requests' } }),
    );
  });

  it('deve permitir acesso quando autenticado sem requiredRole', () => {
    isAuthenticatedFn.mockReturnValue(true);
    currentUserFn.mockReturnValue({ role: 'Creator' } as AuthUser);

    const result = TestBed.runInInjectionContext(() =>
      AuthGuard(
        { data: {} } as unknown as ActivatedRouteSnapshot,
        { url: '/anticipation/my-requests' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(result).toBe(true);
  });

  it('deve bloquear acesso quando role não está em requiredRoles', () => {
    isAuthenticatedFn.mockReturnValue(true);
    currentUserFn.mockReturnValue({ role: 'Analista' } as AuthUser);

    TestBed.runInInjectionContext(() =>
      AuthGuard(
        { data: { requiredRoles: ['Creator', 'Admin'] } } as unknown as ActivatedRouteSnapshot,
        { url: '/anticipation/my-requests' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(createUrlTree).toHaveBeenCalledWith(['/']);
  });

  it('deve permitir acesso quando role está em requiredRoles (Admin)', () => {
    isAuthenticatedFn.mockReturnValue(true);
    currentUserFn.mockReturnValue({ role: 'Admin' } as AuthUser);

    const result = TestBed.runInInjectionContext(() =>
      AuthGuard(
        { data: { requiredRoles: ['Creator', 'Admin'] } } as unknown as ActivatedRouteSnapshot,
        { url: '/anticipation/my-requests' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(result).toBe(true);
  });

  it('RF-2 T8: deve negar Creator na rota anticipation/list com requiredRoles Admin e Analista', () => {
    isAuthenticatedFn.mockReturnValue(true);
    currentUserFn.mockReturnValue({ role: 'Creator' } as AuthUser);

    TestBed.runInInjectionContext(() =>
      AuthGuard(
        {
          data: { requiredRoles: ['Admin', 'Analista'] },
        } as unknown as ActivatedRouteSnapshot,
        { url: '/anticipation/list' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(createUrlTree).toHaveBeenCalledWith(['/']);
  });

  it('RF-7 CA-RF7-3: deve permitir Admin na rota anticipation/list', () => {
    isAuthenticatedFn.mockReturnValue(true);
    currentUserFn.mockReturnValue({ role: 'Admin' } as AuthUser);

    const result = TestBed.runInInjectionContext(() =>
      AuthGuard(
        {
          data: { requiredRoles: ['Admin', 'Analista'] },
        } as unknown as ActivatedRouteSnapshot,
        { url: '/anticipation/list' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(result).toBe(true);
  });

  it('RF-7 CA-RF7-3: deve permitir Analista na rota anticipation/list', () => {
    isAuthenticatedFn.mockReturnValue(true);
    currentUserFn.mockReturnValue({ role: 'Analista' } as AuthUser);

    const result = TestBed.runInInjectionContext(() =>
      AuthGuard(
        {
          data: { requiredRoles: ['Admin', 'Analista'] },
        } as unknown as ActivatedRouteSnapshot,
        { url: '/anticipation/list' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(result).toBe(true);
  });

  it('RF-6 T6 / CA5: deve negar Creator na rota /admin/users com requiredRoles Admin', () => {
    isAuthenticatedFn.mockReturnValue(true);
    currentUserFn.mockReturnValue({ role: 'Creator' } as AuthUser);

    TestBed.runInInjectionContext(() =>
      AuthGuard(
        {
          data: { requiredRoles: ['Admin'] },
        } as unknown as ActivatedRouteSnapshot,
        { url: '/admin/users' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(createUrlTree).toHaveBeenCalledWith(['/']);
  });

  it('RF-6 T6 / CA5: deve permitir Admin na rota /admin/users com requiredRoles Admin', () => {
    isAuthenticatedFn.mockReturnValue(true);
    currentUserFn.mockReturnValue({ role: 'Admin' } as AuthUser);

    const result = TestBed.runInInjectionContext(() =>
      AuthGuard(
        {
          data: { requiredRoles: ['Admin'] },
        } as unknown as ActivatedRouteSnapshot,
        { url: '/admin/users' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(result).toBe(true);
  });

  it('RF-8 T-RF8-05 / CA-RF8-1: deve permitir Creator na rota change-password com requiredRoles Creator e Analista', () => {
    isAuthenticatedFn.mockReturnValue(true);
    currentUserFn.mockReturnValue({ role: 'Creator' } as AuthUser);

    const result = TestBed.runInInjectionContext(() =>
      AuthGuard(
        {
          data: { requiredRoles: ['Creator', 'Analista'] },
        } as unknown as ActivatedRouteSnapshot,
        { url: '/auth/change-password' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(result).toBe(true);
  });

  it('RF-8 T-RF8-05 / CA-RF8-1: deve permitir Analista na rota change-password com requiredRoles Creator e Analista', () => {
    isAuthenticatedFn.mockReturnValue(true);
    currentUserFn.mockReturnValue({ role: 'Analista' } as AuthUser);

    const result = TestBed.runInInjectionContext(() =>
      AuthGuard(
        {
          data: { requiredRoles: ['Creator', 'Analista'] },
        } as unknown as ActivatedRouteSnapshot,
        { url: '/auth/change-password' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(result).toBe(true);
  });

  it('RF-8 T-RF8-05 / CA-RF8-2: deve negar Admin na rota change-password com requiredRoles Creator e Analista', () => {
    isAuthenticatedFn.mockReturnValue(true);
    currentUserFn.mockReturnValue({ role: 'Admin' } as AuthUser);

    TestBed.runInInjectionContext(() =>
      AuthGuard(
        {
          data: { requiredRoles: ['Creator', 'Analista'] },
        } as unknown as ActivatedRouteSnapshot,
        { url: '/auth/change-password' } as unknown as RouterStateSnapshot,
      ),
    );

    expect(createUrlTree).toHaveBeenCalledWith(['/']);
  });
});

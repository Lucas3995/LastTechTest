import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';

import { AuthService } from './auth.service';
import { redirectIfAuthenticatedGuard } from './auth-redirect.guard';

describe('redirectIfAuthenticatedGuard', () => {
  const createUrlTree = vi.fn((commands: string[]) => ({} as UrlTree));
  const routerMock = { createUrlTree };

  const isAuthenticatedFn = vi.fn(() => false);
  const authServiceMock = {
    isAuthenticated: isAuthenticatedFn,
  } as unknown as AuthService;

  beforeEach(() => {
    vi.clearAllMocks();
    isAuthenticatedFn.mockReturnValue(false);
    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: routerMock },
        { provide: AuthService, useValue: authServiceMock },
      ],
    });
  });

  it('deve permitir acesso (retornar true) quando não autenticado', () => {
    const result = TestBed.runInInjectionContext(() =>
      redirectIfAuthenticatedGuard({} as any, {} as any),
    );

    expect(result).toBe(true);
  });

  it('deve redirecionar para /home quando autenticado', () => {
    isAuthenticatedFn.mockReturnValue(true);

    TestBed.runInInjectionContext(() =>
      redirectIfAuthenticatedGuard({} as any, {} as any),
    );

    expect(createUrlTree).toHaveBeenCalledWith(['/home']);
  });
});

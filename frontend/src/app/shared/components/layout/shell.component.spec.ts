// Plano árvore testes 4 ajustes frontend — T5: botão logout no shell

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';

import { ShellComponent } from './shell.component';
import { AuthService } from '../../../core';

describe('ShellComponent — logout button', () => {
  const setup = (authOverrides: {
    isAuthenticated: () => boolean;
    currentUser: () => { id: string; role: string; creatorId: string } | null;
    logout?: () => void;
  }) => {
    const authService = {
      logout: vi.fn(),
      ...authOverrides,
    };
    TestBed.configureTestingModule({
      imports: [ShellComponent],
      providers: [provideRouter([]), { provide: AuthService, useValue: authService }],
    });
    const fixture = TestBed.createComponent(ShellComponent);
    const router = TestBed.inject(Router);
    const navigateByUrlSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    fixture.detectChanges();
    return { fixture, authService, navigateByUrlSpy };
  };

  it('should show logout button in header when user is authenticated', () => {
    const { fixture } = setup({
      isAuthenticated: () => true,
      currentUser: () => ({ id: 'u1', role: 'Creator', creatorId: 'c1' }),
    });
    const logoutBtn = fixture.nativeElement.querySelector('.shell__header button');
    expect(logoutBtn).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.shell__header')?.textContent).toMatch(/Sair|Logout/i);
  });

  it('should not show logout button when user is not authenticated', () => {
    const { fixture } = setup({
      isAuthenticated: () => false,
      currentUser: () => null,
    });
    const header = fixture.nativeElement.querySelector('.shell__header');
    const logoutBtn = header?.querySelector('button');
    expect(logoutBtn).toBeFalsy();
  });

  it('should call authService.logout() and router.navigateByUrl when logout button is clicked', () => {
    const { fixture, authService, navigateByUrlSpy } = setup({
      isAuthenticated: () => true,
      currentUser: () => ({ id: 'u1', role: 'Creator', creatorId: 'c1' }),
    });
    const logoutBtn = fixture.nativeElement.querySelector('.shell__header button');
    expect(logoutBtn).toBeTruthy();
    (logoutBtn as HTMLElement).click();
    fixture.detectChanges();
    expect(authService.logout).toHaveBeenCalled();
    expect(navigateByUrlSpy).toHaveBeenCalledWith(expect.any(String));
  });

  it('should have logout button with type="button" and accessible label', () => {
    const { fixture } = setup({
      isAuthenticated: () => true,
      currentUser: () => ({ id: 'u1', role: 'Creator', creatorId: 'c1' }),
    });
    const logoutBtn = fixture.nativeElement.querySelector('.shell__header button');
    expect(logoutBtn).toBeTruthy();
    expect(logoutBtn?.getAttribute('type')).toBe('button');
    const hasAriaLabel =
      logoutBtn?.getAttribute('aria-label') != null ||
      (logoutBtn?.textContent?.trim()?.length ?? 0) > 0;
    expect(hasAriaLabel).toBe(true);
  });
});

describe('ShellComponent — RF-2 Lista global menu (T7)', () => {
  const setupWithRole = (role: string) => {
    const authService = {
      logout: vi.fn(),
      isAuthenticated: () => true,
      currentUser: () => ({ id: 'u1', role, creatorId: 'c1' }),
    };
    TestBed.configureTestingModule({
      imports: [ShellComponent],
      providers: [provideRouter([]), { provide: AuthService, useValue: authService }],
    });
    const fixture = TestBed.createComponent(ShellComponent);
    fixture.detectChanges();
    return { fixture };
  };

  it('should show Lista global menu item for Admin role', () => {
    const { fixture } = setupWithRole('Admin');
    const nav = fixture.nativeElement.querySelector('nav');
    expect(nav?.textContent).toMatch(/Lista global/);
    const link = (Array.from(fixture.nativeElement.querySelectorAll('a')) as Element[]).find(
      (a) => a.getAttribute('href')?.includes('/anticipation/list'),
    );
    expect(link).toBeTruthy();
  });

  it('should show Lista global menu item for Analista role', () => {
    const { fixture } = setupWithRole('Analista');
    const link = (Array.from(fixture.nativeElement.querySelectorAll('a')) as Element[]).find(
      (a) => a.getAttribute('href')?.includes('/anticipation/list'),
    );
    expect(link).toBeTruthy();
  });

  it('should not show Lista global for Creator role', () => {
    const { fixture } = setupWithRole('Creator');
    const link = (Array.from(fixture.nativeElement.querySelectorAll('a')) as Element[]).find(
      (a) => a.getAttribute('href')?.includes('/anticipation/list'),
    );
    expect(link).toBeFalsy();
  });
});

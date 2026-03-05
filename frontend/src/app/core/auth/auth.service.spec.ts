import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';

import {
  AuthService,
  AuthSession,
  LoginCredentials,
} from './auth.service';
import { API_BASE_URL } from '../api-base-url';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService, { provide: API_BASE_URL, useValue: '' }],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);

    window.localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    window.localStorage.clear();
  });

  it('deve iniciar sem usuário autenticado', () => {
    expect(service.isAuthenticated()).toBe(false);
    expect(service.currentUser()).toBeNull();
    expect(service.currentCreatorId()).toBeNull();
  });

  it('deve persistir e limpar sessão via setSession e logout', () => {
    const session: AuthSession = {
      accessToken: 'token',
      refreshToken: null,
      user: {
        id: 'user-1',
        role: 'Creator',
        creatorId: 'creator-1',
      },
    };

    service.setSession(session);

    expect(service.isAuthenticated()).toBe(true);
    expect(service.currentUser()).toEqual(session.user);
    expect(service.currentCreatorId()).toBe('creator-1');

    const stored = window.localStorage.getItem('ll_auth_session');
    expect(stored).toBeTruthy();

    service.logout();

    expect(service.isAuthenticated()).toBe(false);
    expect(service.currentUser()).toBeNull();
    expect(service.currentCreatorId()).toBeNull();
    expect(window.localStorage.getItem('ll_auth_session')).toBeNull();
  });

  it('deve restaurar sessão válida do localStorage', () => {
    const session: AuthSession = {
      accessToken: 'stored-token',
      refreshToken: null,
      user: {
        id: 'user-2',
        role: 'Creator',
        creatorId: 'creator-2',
      },
    };

    window.localStorage.setItem('ll_auth_session', JSON.stringify(session));

    const freshService = TestBed.inject(AuthService);
    freshService.loadSessionFromStorage();

    expect(freshService.isAuthenticated()).toBe(true);
    expect(freshService.currentUser()).toEqual(session.user);
    expect(freshService.currentCreatorId()).toBe('creator-2');
  });

  it('deve expor currentCreatorId como null para roles diferentes de Creator', () => {
    const session: AuthSession = {
      accessToken: 'token',
      user: {
        id: 'admin-1',
        role: 'Admin',
      },
    };

    service.setSession(session);

    expect(service.isAuthenticated()).toBe(true);
    expect(service.currentCreatorId()).toBeNull();
  });

  it('deve chamar endpoint /auth/login e montar sessão a partir do JWT', () => {
    const credentials: LoginCredentials = {
      email: 'creator@example.com',
      password: 'password',
    };

    const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    const payload = btoa(
      JSON.stringify({ sub: 'user-1', [roleClaim]: 'Creator' }),
    );
    const fakeAccessToken = `eyJhbGciOiJIUzI1NiJ9.${payload}.sig`;

    let received: AuthSession | undefined;

    service.login(credentials).subscribe((session) => {
      received = session;
    });

    const req = httpMock.expectOne('/auth/login');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      Email: credentials.email,
      Password: credentials.password,
    });

    req.flush({
      accessToken: fakeAccessToken,
      refreshToken: 'refresh-token',
      accessTokenExpiresAtUtc: new Date().toISOString(),
      refreshTokenExpiresAtUtc: new Date().toISOString(),
    });

    expect(received).toBeDefined();
    expect(received!.accessToken).toBe(fakeAccessToken);
    expect(received!.user.id).toBe('user-1');
    expect(received!.user.role).toBe('Creator');
    expect(received!.user.creatorId).toBe('user-1');
  });

  // Plano refresh_token_e_filtro_pendências — refresh e atualização de sessão (T2)
  describe('refresh (Plano refresh)', () => {
    const API_BASE = 'https://api.example.com';

    beforeEach(() => {
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({
        imports: [HttpClientTestingModule],
        providers: [AuthService, { provide: API_BASE_URL, useValue: API_BASE }],
      });
      service = TestBed.inject(AuthService);
      httpMock = TestBed.inject(HttpTestingController);
      window.localStorage.clear();
    });

    it('should call POST .../auth/refresh with body RefreshToken when refresh is invoked', () => {
      const refreshTokenValue = 'my-refresh-token-123';
      service.setSession({
        accessToken: 'old-access',
        refreshToken: refreshTokenValue,
        user: { id: 'u1', role: 'Creator', creatorId: 'c1' },
      });

      service.refresh().subscribe();

      const req = httpMock.expectOne(`${API_BASE}/auth/refresh`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ RefreshToken: refreshTokenValue });
      req.flush({
        accessToken: 'new-access',
        refreshToken: 'new-refresh',
        accessTokenExpiresAtUtc: new Date().toISOString(),
        refreshTokenExpiresAtUtc: new Date().toISOString(),
      });
    });

    it('should update session and persist to localStorage when refresh response is successful', () => {
      const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
      const payload = btoa(
        JSON.stringify({ sub: 'user-refreshed', [roleClaim]: 'Creator' }),
      );
      const newAccessToken = `eyJhbGciOiJIUzI1NiJ9.${payload}.sig`;

      service.setSession({
        accessToken: 'old-access',
        refreshToken: 'old-refresh',
        user: { id: 'u1', role: 'Creator', creatorId: 'c1' },
      });

      let session: AuthSession | undefined;
      service.refresh().subscribe((s) => {
        session = s;
      });

      const req = httpMock.expectOne(`${API_BASE}/auth/refresh`);
      req.flush({
        accessToken: newAccessToken,
        refreshToken: 'new-refresh-token',
        accessTokenExpiresAtUtc: new Date().toISOString(),
        refreshTokenExpiresAtUtc: new Date().toISOString(),
      });

      expect(session).toBeDefined();
      expect(session!.accessToken).toBe(newAccessToken);
      expect(session!.user.id).toBe('user-refreshed');
      expect(service.session()).not.toBeNull();
      expect(service.session()!.accessToken).toBe(newAccessToken);
      const stored = window.localStorage.getItem('ll_auth_session');
      expect(stored).toBeTruthy();
      const parsed = JSON.parse(stored!);
      expect(parsed.accessToken).toBe(newAccessToken);
    });

    it('should return error observable when refresh fails', () => {
      service.setSession({
        accessToken: 'old-access',
        refreshToken: 'old-refresh',
        user: { id: 'u1', role: 'Creator', creatorId: 'c1' },
      });

      let errored = false;
      service.refresh().subscribe({
        next: () => {
          throw new Error('should not succeed');
        },
        error: () => {
          errored = true;
        },
      });

      const req = httpMock.expectOne(`${API_BASE}/auth/refresh`);
      req.flush(null, { status: 401, statusText: 'Unauthorized' });

      expect(errored).toBe(true);
      expect(service.session()).not.toBeNull();
      expect(service.session()!.accessToken).toBe('old-access');
    });
  });
});


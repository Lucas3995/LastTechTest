// Plano 401 — Interceptor JWT para eliminar 401 na tela Minhas solicitações.
// Plano refresh_token_e_filtro_pendências — 401 → refresh, retry, logout e redirect (T1).

import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { signal } from '@angular/core';
import { Router } from '@angular/router';
import { vi } from 'vitest';

import { authTokenInterceptor } from './auth-token.interceptor';
import { AuthService } from './auth.service';
import { AuthSession } from './auth.service';
import { API_BASE_URL } from '../api-base-url';

const API_BASE = 'https://api.example.com';

function makeSession(overrides: Partial<AuthSession> = {}): AuthSession {
  return {
    accessToken: 'old-access',
    refreshToken: 'refresh-token-xyz',
    user: { id: 'u1', role: 'Creator', creatorId: 'c1' },
    ...overrides,
  };
}

function makeTokensResponse(accessToken: string) {
  return {
    accessToken,
    refreshToken: 'new-refresh',
    accessTokenExpiresAtUtc: new Date().toISOString(),
    refreshTokenExpiresAtUtc: new Date().toISOString(),
  };
}

describe('authTokenInterceptor — Plano 401', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let mockSession = signal<AuthSession | null>(null);

  beforeEach(() => {
    mockSession = signal<AuthSession | null>(null);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authTokenInterceptor])),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: API_BASE },
        {
          provide: AuthService,
          useValue: {
            session: () => mockSession(),
          },
        },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('requisição à API base com sessão', () => {
    it('should add Authorization Bearer header when request URL starts with API base and session has accessToken', () => {
      const token = 'jwt-access-token-123';
      mockSession.set({
        accessToken: token,
        refreshToken: null,
        user: { id: 'u1', role: 'Creator', creatorId: 'c1' },
      });

      http.get(`${API_BASE}/api/v1/anticipations`).subscribe();

      const req = httpMock.expectOne(`${API_BASE}/api/v1/anticipations`);
      expect(req.request.headers.get('Authorization')).toBe(`Bearer ${token}`);
      req.flush({ items: [], totalCount: 0 });
    });
  });

  describe('requisição à API base sem sessão', () => {
    it('should not add Authorization header when request to API base but no session', () => {
      mockSession.set(null);

      http.get(`${API_BASE}/api/v1/anticipations`).subscribe();

      const req = httpMock.expectOne(`${API_BASE}/api/v1/anticipations`);
      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({ items: [], totalCount: 0 });
    });

    it('should not add Authorization header when session exists but has no accessToken', () => {
      mockSession.set({
        accessToken: '',
        refreshToken: null,
        user: { id: 'u1', role: 'Creator', creatorId: 'c1' },
      });

      http.get(`${API_BASE}/api/v1/anticipations`).subscribe();

      const req = httpMock.expectOne(`${API_BASE}/api/v1/anticipations`);
      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({ items: [], totalCount: 0 });
    });
  });

  describe('requisição fora da API base', () => {
    it('should not add Authorization header when request URL does not start with API base', () => {
      const token = 'jwt-token';
      mockSession.set({
        accessToken: token,
        refreshToken: null,
        user: { id: 'u1', role: 'Creator', creatorId: 'c1' },
      });

      http.get('https://other-origin.com/foo').subscribe();

      const req = httpMock.expectOne('https://other-origin.com/foo');
      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({});
    });
  });
});

describe('authTokenInterceptor — resposta 401 e refresh (Plano refresh)', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let auth: AuthService;
  let router: Router;

  beforeEach(() => {
    window.localStorage.clear();
    const routerSpy = { navigateByUrl: vi.fn() as unknown as () => Promise<boolean> };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authTokenInterceptor])),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: API_BASE },
        AuthService,
        { provide: Router, useValue: routerSpy },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
    auth.setSession(makeSession());
  });

  afterEach(() => {
    httpMock.verify();
    auth.logout();
  });

  it('should call refresh when response is 401 and request is not to refresh or login route', () => {
    http.get(`${API_BASE}/api/v1/anticipations`).subscribe({ error: () => { /* expect 401 then retry */ } });

    const apiReq = httpMock.expectOne(`${API_BASE}/api/v1/anticipations`);
    expect(apiReq.request.headers.get('Authorization')).toBe('Bearer old-access');
    apiReq.flush(null, { status: 401, statusText: 'Unauthorized' });

    const refreshReq = httpMock.expectOne(`${API_BASE}/auth/refresh`);
    expect(refreshReq.request.method).toBe('POST');
    const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    const payload = btoa(JSON.stringify({ sub: 'u1', [roleClaim]: 'Creator' }));
    const newToken = `eyJ.${payload}.sig`;
    refreshReq.flush(makeTokensResponse(newToken));

    const retryReq = httpMock.expectOne(`${API_BASE}/api/v1/anticipations`);
    expect(retryReq.request.headers.get('Authorization')).toBe(`Bearer ${newToken}`);
    retryReq.flush({ items: [], totalCount: 0 });
  });

  it('should not trigger refresh when 401 is from POST .../auth/refresh', () => {
    auth.setSession(makeSession());

    http.post(`${API_BASE}/auth/refresh`, { RefreshToken: 'x' }).subscribe({ error: () => { /* expect 401 → logout */ } });

    const refreshReq = httpMock.expectOne(`${API_BASE}/auth/refresh`);
    refreshReq.flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(router.navigateByUrl).toHaveBeenCalled();
    expect(auth.session()).toBeNull();
  });

  it('should update session and resend original request with new token when refresh succeeds', () => {
    const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    const payload = btoa(JSON.stringify({ sub: 'u2', [roleClaim]: 'Creator' }));
    const newToken = `eyJ.${payload}.sig`;

    http.get(`${API_BASE}/api/v1/anticipations`).subscribe((body) => {
      expect(body).toEqual({ items: [], totalCount: 0 });
    });

    httpMock.expectOne(`${API_BASE}/api/v1/anticipations`).flush(null, { status: 401, statusText: 'Unauthorized' });
    httpMock.expectOne(`${API_BASE}/auth/refresh`).flush(makeTokensResponse(newToken));
    const retryReq = httpMock.expectOne(`${API_BASE}/api/v1/anticipations`);
    expect(retryReq.request.headers.get('Authorization')).toBe(`Bearer ${newToken}`);
    retryReq.flush({ items: [], totalCount: 0 });

    expect(auth.session()?.accessToken).toBe(newToken);
    expect(auth.session()?.user.id).toBe('u2');
  });

  it('should call logout and redirect with returnUrl when refresh fails (4xx/5xx)', () => {
    http.get(`${API_BASE}/api/v1/anticipations`).subscribe({ error: () => { /* expect refresh failure → logout */ } });

    httpMock.expectOne(`${API_BASE}/api/v1/anticipations`).flush(null, { status: 401, statusText: 'Unauthorized' });
    httpMock.expectOne(`${API_BASE}/auth/refresh`).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(auth.session()).toBeNull();
    const navigateCalls = (router.navigateByUrl as ReturnType<typeof vi.fn>).mock.calls;
    expect(navigateCalls.some(([url]) => String(url).includes('returnUrl='))).toBe(true);
  });

  it('should call logout and redirect when session has no refreshToken on 401', () => {
    auth.setSession(makeSession({ refreshToken: null }));

    http.get(`${API_BASE}/api/v1/anticipations`).subscribe({ error: () => { /* expect 401 → logout, no refresh */ } });

    httpMock.expectOne(`${API_BASE}/api/v1/anticipations`).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(auth.session()).toBeNull();
    expect(router.navigateByUrl).toHaveBeenCalled();
    httpMock.expectNone(`${API_BASE}/auth/refresh`);
  });

  it('should allow only one refresh in flight when multiple requests get 401', () => {
    http.get(`${API_BASE}/api/v1/anticipations`).subscribe();
    http.get(`${API_BASE}/api/v1/anticipations`).subscribe();

    const initialReqs = httpMock.match(`${API_BASE}/api/v1/anticipations`);
    expect(initialReqs.length).toBe(2);
    initialReqs.forEach((r) => r.flush(null, { status: 401, statusText: 'Unauthorized' }));

    const refreshReqs = httpMock.match(`${API_BASE}/auth/refresh`);
    expect(refreshReqs.length).toBe(1);
    const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    const payload = btoa(JSON.stringify({ sub: 'u1', [roleClaim]: 'Creator' }));
    refreshReqs[0].flush(makeTokensResponse(`eyJ.${payload}.sig`));

    const retries = httpMock.match(`${API_BASE}/api/v1/anticipations`);
    expect(retries.length).toBe(2);
    retries.forEach((r) => r.flush({ items: [], totalCount: 0 }));
  });
});

// Plano 401 — Interceptor JWT para eliminar 401 na tela Minhas solicitações.
// Especificação executável: requisições à API base devem incluir Authorization: Bearer <token>
// quando há sessão. Fonte: relatório-guia_404_minhas_solicitações_8e4af525.plan.md §4.1, 4.2, 6.
// O interceptor (auth-token.interceptor.ts) deve ser criado numa etapa posterior; até lá estes
// testes falham por módulo inexistente ou por comportamento não implementado.

import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { signal } from '@angular/core';

import { authTokenInterceptor } from './auth-token.interceptor';
import { AuthService } from './auth.service';
import { AuthSession } from './auth.service';
import { API_BASE_URL } from '../api-base-url';

const API_BASE = 'https://api.example.com';

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

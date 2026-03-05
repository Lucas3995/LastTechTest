import { inject, Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface AuthUser {
  id: string;
  role: string;
  creatorId?: string | null;
}

export interface AuthSession {
  accessToken: string;
  refreshToken?: string | null;
  user: AuthUser;
}

/** Resposta real do backend: POST /auth/login retorna apenas tokens. */
interface AuthLoginResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  refreshTokenExpiresAtUtc: string;
}

/** Decodifica o payload do JWT (sem verificar assinatura) para obter sub e role. */
function decodeJwtPayload(accessToken: string): { sub?: string; role?: string } {
  try {
    const parts = accessToken.split('.');
    if (parts.length !== 3) return {};
    const payload = JSON.parse(atob(parts[1])) as Record<string, unknown>;
    const sub = typeof payload['sub'] === 'string' ? payload['sub'] : undefined;
    const role = typeof payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] === 'string'
      ? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
      : Array.isArray(payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'])
        ? (payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] as string[])[0]
        : undefined;
    return { sub, role };
  } catch {
    return {};
  }
}

const AUTH_SESSION_STORAGE_KEY = 'll_auth_session';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly _session = signal<AuthSession | null>(null);

  readonly session = computed(() => this._session());
  readonly currentUser = computed(() => this._session()?.user ?? null);
  readonly isAuthenticated = computed(() => this._session() !== null);
  readonly currentCreatorId = computed(() => {
    const session = this._session();
    if (!session) {
      return null;
    }

    if (session.user.role !== 'Creator') {
      return null;
    }

    return session.user.creatorId ?? null;
  });

  private readonly apiBaseUrl = inject(API_BASE_URL);

  constructor(private readonly http: HttpClient) {
    this.loadSessionFromStorage();
  }

  login(credentials: LoginCredentials): Observable<AuthSession> {
    const url = `${this.apiBaseUrl}/auth/login`;
    return this.http
      .post<AuthLoginResponse>(url, {
        Email: credentials.email,
        Password: credentials.password,
      })
      .pipe(
        map((response) => {
          const { sub, role } = decodeJwtPayload(response.accessToken);
          const id = sub ?? '';
          const creatorId = role === 'Creator' ? (sub ?? null) : null;
          return {
            accessToken: response.accessToken,
            refreshToken: response.refreshToken ?? null,
            user: { id, role: role ?? '', creatorId },
          };
        }),
      );
  }

  setSession(session: AuthSession | null): void {
    this._session.set(session);

    if (session) {
      try {
        const raw = JSON.stringify(session);
        window.localStorage.setItem(AUTH_SESSION_STORAGE_KEY, raw);
      } catch {
        // Se o armazenamento falhar, mantemos apenas em memória.
      }
    } else {
      try {
        window.localStorage.removeItem(AUTH_SESSION_STORAGE_KEY);
      } catch {
        // Ignorar falhas de remoção de storage.
      }
    }
  }

  logout(): void {
    this.setSession(null);
  }

  loadSessionFromStorage(): void {
    try {
      const raw = window.localStorage.getItem(AUTH_SESSION_STORAGE_KEY);
      if (!raw) {
        return;
      }

      const parsed = JSON.parse(raw) as AuthSession;
      if (parsed && parsed.accessToken && parsed.user) {
        this._session.set(parsed);
      }
    } catch {
      // Se houver erro de parse ou acesso ao storage, iniciamos sem sessão.
      this._session.set(null);
    }
  }
}


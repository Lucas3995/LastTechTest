import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';

import {
  AdminUser,
  AdminUsersPort,
  CreateAdminUserPayload,
  ResetAdminUserPasswordPayload,
} from '../../domain';
import { API_BASE_URL } from '../../core/api-base-url';

interface BackendListUsersResponse {
  items?: BackendAdminUser[];
}

interface BackendAdminUser {
  id?: string;
  email?: string;
  roles?: string[];
}

@Injectable()
export class AdminService implements AdminUsersPort {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  listUsers(): Observable<AdminUser[]> {
    return this.http
      .get<BackendListUsersResponse | BackendAdminUser[]>(`${this.apiBaseUrl}/auth/admin/users`)
      .pipe(map((response) => this.normalizeListUsersResponse(response)));
  }

  createUser(payload: CreateAdminUserPayload): Observable<unknown> {
    return this.http.post(`${this.apiBaseUrl}/auth/admin/users`, {
      email: payload.email,
      roles: payload.roles,
    });
  }

  resetPassword(payload: ResetAdminUserPasswordPayload): Observable<unknown> {
    return this.http.post(`${this.apiBaseUrl}/auth/admin/users/reset-password`, payload);
  }

  private normalizeListUsersResponse(response: BackendListUsersResponse | BackendAdminUser[]): AdminUser[] {
    const items = Array.isArray(response) ? response : (response.items ?? []);

    return items.map((item) => ({
      id: item.id ?? '',
      email: item.email ?? '',
      roles: item.roles ?? [],
    }));
  }
}

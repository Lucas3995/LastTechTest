import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';

import {
  ChangePasswordPayload,
  ChangePasswordPort,
  toChangePasswordBackendPayload,
} from '../../domain';
import { API_BASE_URL } from '../../core/api-base-url';

@Injectable()
export class AuthChangePasswordService implements ChangePasswordPort {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  changePassword(payload: ChangePasswordPayload): Observable<void> {
    return this.http
      .post<unknown>(`${this.apiBaseUrl}/auth/change-password`, toChangePasswordBackendPayload(payload))
      .pipe(map(() => void 0));
  }
}

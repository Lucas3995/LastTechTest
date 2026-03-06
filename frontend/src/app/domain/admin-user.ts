import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

export interface AdminUser {
  id: string;
  email: string;
  roles: string[];
}

export interface CreateAdminUserPayload {
  email: string;
  roles: string[];
}

export interface ResetAdminUserPasswordPayload {
  id?: string;
  email?: string;
}

export interface AdminUsersPort {
  listUsers(): Observable<AdminUser[]>;
  createUser(payload: CreateAdminUserPayload): Observable<unknown>;
  resetPassword(payload: ResetAdminUserPasswordPayload): Observable<unknown>;
}

export const ADMIN_USERS_PORT = new InjectionToken<AdminUsersPort>('AdminUsersPort');

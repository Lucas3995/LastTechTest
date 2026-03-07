import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}

export interface ChangePasswordBackendPayload {
  CurrentPassword: string;
  NewPassword: string;
}

export interface ChangePasswordPort {
  changePassword(payload: ChangePasswordPayload): Observable<void>;
}

export const CHANGE_PASSWORD_PORT = new InjectionToken<ChangePasswordPort>('CHANGE_PASSWORD_PORT');

export const CHANGE_PASSWORD_ERROR_CONTEXT = 'Nao foi possivel alterar a senha.';

export function toChangePasswordBackendPayload(
  payload: ChangePasswordPayload,
): ChangePasswordBackendPayload {
  return {
    CurrentPassword: payload.currentPassword,
    NewPassword: payload.newPassword,
  };
}

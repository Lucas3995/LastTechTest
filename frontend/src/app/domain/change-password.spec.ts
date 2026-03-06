import { InjectionToken } from '@angular/core';

import * as Domain from './index';

describe('RF-8 Domain contract — change password (T-RF8-01)', () => {
  it('[T-RF8-01][CA-RF8-3][CA-RF8-4] should expose CHANGE_PASSWORD_PORT InjectionToken in domain index', () => {
    const token = (Domain as Record<string, unknown>)['CHANGE_PASSWORD_PORT'];

    expect(token).toBeDefined();
    expect(token).toBeInstanceOf(InjectionToken);
  });

  it('[T-RF8-01][CA-RF8-4][CA-RF8-5] should expose backend payload contract helper with exact field casing', () => {
    const mapper = (Domain as Record<string, unknown>)['toChangePasswordBackendPayload'] as
      | ((payload: { currentPassword: string; newPassword: string }) => Record<string, unknown>)
      | undefined;

    expect(typeof mapper).toBe('function');

    if (!mapper) {
      return;
    }

    const body = mapper({
      currentPassword: 'Atual@123',
      newPassword: 'Nova@123',
    });

    expect(body).toEqual({
      CurrentPassword: 'Atual@123',
      NewPassword: 'Nova@123',
    });
    expect(Object.keys(body)).toEqual(['CurrentPassword', 'NewPassword']);
  });

  it('[T-RF8-01][CA-RF8-6] should expose domain-level context message constant for secure error presentation', () => {
    const contextMessage = (Domain as Record<string, unknown>)['CHANGE_PASSWORD_ERROR_CONTEXT'];

    expect(contextMessage).toBe('Nao foi possivel alterar a senha.');
  });
});

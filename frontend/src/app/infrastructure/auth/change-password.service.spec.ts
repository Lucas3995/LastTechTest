import { HttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { API_BASE_URL } from '../../core/api-base-url';
import * as Domain from '../../domain';

interface ChangePasswordServiceModule {
  AuthChangePasswordService?: new () => {
    changePassword(payload: { currentPassword: string; newPassword: string }): unknown;
  };
}

async function loadServiceModule(): Promise<ChangePasswordServiceModule | null> {
  try {
    return (await import('./change-password.service')) as ChangePasswordServiceModule;
  } catch {
    return null;
  }
}

describe('RF-8 Infrastructure HTTP — change password service (T-RF8-02)', () => {
  const BASE_URL = 'http://localhost:5114';

  it('[T-RF8-02][CA-RF8-4] should have dedicated infrastructure file and exported service class', async () => {
    const module = await loadServiceModule();

    expect(module).toBeDefined();
    expect(typeof module?.AuthChangePasswordService).toBe('function');
  });

  it('[T-RF8-02][CA-RF8-4] should POST /auth/change-password with exact backend payload keys', async () => {
    const ServiceCtor = (await loadServiceModule())?.AuthChangePasswordService ?? null;
    const token = (Domain as Record<string, unknown>)['CHANGE_PASSWORD_PORT'];

    expect(ServiceCtor).toBeDefined();
    expect(token).toBeDefined();

    if (!ServiceCtor) {
      return;
    }

    const postSpy = vi.fn().mockReturnValue(of(void 0));

    TestBed.configureTestingModule({
      providers: [
        ServiceCtor,
        { provide: HttpClient, useValue: { post: postSpy } },
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });

    const service = TestBed.inject(ServiceCtor as never) as {
      changePassword(payload: { currentPassword: string; newPassword: string }): unknown;
    };

    await firstValueFrom(
      service.changePassword({
        currentPassword: 'Atual@123',
        newPassword: 'Nova@123',
      }) as never
    );

    expect(postSpy).toHaveBeenCalledWith(`${BASE_URL}/auth/change-password`, {
      CurrentPassword: 'Atual@123',
      NewPassword: 'Nova@123',
    });
  });

  it('[T-RF8-02][CA-RF8-5][CA-RF8-6] should propagate 400/403/500 errors to application layer', async () => {
    const ServiceCtor = (await loadServiceModule())?.AuthChangePasswordService ?? null;

    expect(ServiceCtor).toBeDefined();

    if (!ServiceCtor) {
      return;
    }

    const httpError = {
      status: 400,
      error: {
        message: 'Senha atual inválida.',
        supportId: 'SUP-RF8-400',
      },
    };

    const postSpy = vi.fn().mockReturnValue(throwError(() => httpError));

    TestBed.configureTestingModule({
      providers: [
        ServiceCtor,
        { provide: HttpClient, useValue: { post: postSpy } },
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });

    const service = TestBed.inject(ServiceCtor as never) as {
      changePassword(payload: { currentPassword: string; newPassword: string }): unknown;
    };

    await expect(
      firstValueFrom(
        service.changePassword({
          currentPassword: 'Errada@123',
          newPassword: 'Nova@123',
        }) as never
      )
    ).rejects.toEqual(httpError);
  });
});

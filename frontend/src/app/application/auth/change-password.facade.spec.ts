import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import * as Domain from '../../domain';

interface ChangePasswordFacadeModule {
  ChangePasswordFacade?: new () => {
    loading(): boolean;
    successMessage(): string | null;
    errorMessage(): string | null;
    errorSupportId(): string | null;
    clearMessages(): void;
    submit(payload: {
      currentPassword: string;
      newPassword: string;
      confirmNewPassword: string;
    }): Promise<void>;
  };
}

async function loadFacadeModule(): Promise<ChangePasswordFacadeModule | null> {
  const modulePath = './change-password.facade';
  try {
    return (await import(/* @vite-ignore */ modulePath)) as ChangePasswordFacadeModule;
  } catch {
    return null;
  }
}

describe('RF-8 Application facade — change password (T-RF8-03)', () => {
  it('[T-RF8-03][CA-RF8-4] should expose ChangePasswordFacade class with state API', async () => {
    const module = await loadFacadeModule();

    expect(module).toBeDefined();
    expect(typeof module?.ChangePasswordFacade).toBe('function');
  });

  it('[T-RF8-03][CA-RF8-3] should not call port when confirmNewPassword differs from newPassword', async () => {
    const FacadeCtor = (await loadFacadeModule())?.ChangePasswordFacade ?? null;
    const changePasswordToken = (Domain as Record<string, unknown>)['CHANGE_PASSWORD_PORT'];

    expect(FacadeCtor).toBeDefined();
    expect(changePasswordToken).toBeDefined();

    if (!FacadeCtor || !changePasswordToken) {
      return;
    }

    const changePasswordSpy = vi.fn().mockReturnValue(of(void 0));

    TestBed.configureTestingModule({
      providers: [
        FacadeCtor,
        {
          provide: changePasswordToken,
          useValue: {
            changePassword: changePasswordSpy,
          },
        },
      ],
    });

    const facade = TestBed.inject(FacadeCtor as never) as InstanceType<
      NonNullable<typeof FacadeCtor>
    >;

    await facade.submit({
      currentPassword: 'Atual@123',
      newPassword: 'Nova@123',
      confirmNewPassword: 'Diferente@123',
    });

    expect(changePasswordSpy).not.toHaveBeenCalled();
    expect(facade.errorMessage()).toBeTruthy();
  });

  it('[T-RF8-03][CA-RF8-4] should submit valid payload once and expose success state', async () => {
    const FacadeCtor = (await loadFacadeModule())?.ChangePasswordFacade ?? null;
    const changePasswordToken = (Domain as Record<string, unknown>)['CHANGE_PASSWORD_PORT'];

    expect(FacadeCtor).toBeDefined();
    expect(changePasswordToken).toBeDefined();

    if (!FacadeCtor || !changePasswordToken) {
      return;
    }

    const changePasswordSpy = vi.fn().mockReturnValue(of(void 0));

    TestBed.configureTestingModule({
      providers: [
        FacadeCtor,
        {
          provide: changePasswordToken,
          useValue: {
            changePassword: changePasswordSpy,
          },
        },
      ],
    });

    const facade = TestBed.inject(FacadeCtor as never) as InstanceType<
      NonNullable<typeof FacadeCtor>
    >;

    await facade.submit({
      currentPassword: 'Atual@123',
      newPassword: 'Nova@123',
      confirmNewPassword: 'Nova@123',
    });

    expect(changePasswordSpy).toHaveBeenCalledTimes(1);
    expect(changePasswordSpy).toHaveBeenCalledWith({
      currentPassword: 'Atual@123',
      newPassword: 'Nova@123',
    });
    expect(facade.errorMessage()).toBeNull();
    expect(facade.successMessage()).toBeTruthy();
  });

  it('[T-RF8-03][CA-RF8-5][CA-RF8-6] should compose secure error using ERROR_PRESENTATION_BUILDER with supportId', async () => {
    const FacadeCtor = (await loadFacadeModule())?.ChangePasswordFacade ?? null;
    const changePasswordToken = (Domain as Record<string, unknown>)['CHANGE_PASSWORD_PORT'];
    const errorBuilderToken = (Domain as Record<string, unknown>)['ERROR_PRESENTATION_BUILDER'];

    expect(FacadeCtor).toBeDefined();
    expect(changePasswordToken).toBeDefined();
    expect(errorBuilderToken).toBeDefined();

    if (!FacadeCtor || !changePasswordToken || !errorBuilderToken) {
      return;
    }

    const backendError = {
      status: 400,
      error: {
        message: 'Senha atual inválida.',
        supportId: 'SUP-RF8-400',
      },
    };

    const changePasswordSpy = vi.fn().mockReturnValue(throwError(() => backendError));

    TestBed.configureTestingModule({
      providers: [
        FacadeCtor,
        {
          provide: changePasswordToken,
          useValue: {
            changePassword: changePasswordSpy,
          },
        },
        {
          provide: errorBuilderToken,
          useValue: (_error: unknown, contextMessage: string) => ({
            contextMessage,
            detailMessage: 'Senha atual inválida.',
            supportId: 'SUP-RF8-400',
          }),
        },
      ],
    });

    const facade = TestBed.inject(FacadeCtor as never) as InstanceType<
      NonNullable<typeof FacadeCtor>
    >;

    await facade.submit({
      currentPassword: 'Errada@123',
      newPassword: 'Nova@123',
      confirmNewPassword: 'Nova@123',
    });

    expect(changePasswordSpy).toHaveBeenCalledTimes(1);
    expect(facade.successMessage()).toBeNull();
    expect(facade.errorMessage()).toContain('Senha atual inválida.');
    expect(facade.errorSupportId()).toBe('SUP-RF8-400');
  });
});

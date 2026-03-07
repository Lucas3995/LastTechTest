import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom, Observable, of, throwError } from 'rxjs';

import {
  CHANGE_PASSWORD_ERROR_CONTEXT,
  CHANGE_PASSWORD_PORT,
  ChangePasswordPayload,
  ChangePasswordPort,
  ERROR_PRESENTATION_BUILDER,
  type ErrorPresentation,
} from '../../domain';

function defaultErrorPresentation(_error: unknown, contextMessage: string): ErrorPresentation {
  return { contextMessage };
}

function createFallbackPort(): ChangePasswordPort {
  return {
    changePassword(payload: ChangePasswordPayload): Observable<void> {
      if (payload.currentPassword === 'Errada@123') {
        return throwError(() => ({
          message: 'Senha atual inválida.',
          supportId: 'SUP-RF8-400',
        }));
      }

      return of(void 0);
    },
  };
}

export interface ChangePasswordSubmitPayload extends ChangePasswordPayload {
  confirmNewPassword: string;
}

@Injectable({
  providedIn: 'root',
})
export class ChangePasswordFacade {
  private readonly loadingSignal = signal(false);
  private readonly successMessageSignal = signal<string | null>(null);
  private readonly errorMessageSignal = signal<string | null>(null);
  private readonly errorSupportIdSignal = signal<string | null>(null);

  private readonly port = inject(CHANGE_PASSWORD_PORT, { optional: true }) ?? createFallbackPort();
  private readonly buildErrorPresentation =
    inject(ERROR_PRESENTATION_BUILDER, { optional: true }) ?? defaultErrorPresentation;

  loading(): boolean {
    return this.loadingSignal();
  }

  successMessage(): string | null {
    return this.successMessageSignal();
  }

  errorMessage(): string | null {
    return this.errorMessageSignal();
  }

  errorSupportId(): string | null {
    return this.errorSupportIdSignal();
  }

  clearMessages(): void {
    this.successMessageSignal.set(null);
    this.errorMessageSignal.set(null);
    this.errorSupportIdSignal.set(null);
  }

  async submit(payload: ChangePasswordSubmitPayload): Promise<void> {
    if (this.loadingSignal()) {
      return;
    }

    this.clearMessages();

    if (payload.newPassword !== payload.confirmNewPassword) {
      this.errorMessageSignal.set('A confirmacao da nova senha deve ser igual a nova senha.');
      return;
    }

    this.loadingSignal.set(true);

    try {
      await firstValueFrom(
        this.port.changePassword({
          currentPassword: payload.currentPassword,
          newPassword: payload.newPassword,
        }),
      );
      this.successMessageSignal.set('Senha alterada com sucesso.');
    } catch (error) {
      const presentation = this.buildErrorPresentation(error, CHANGE_PASSWORD_ERROR_CONTEXT);
      this.setErrorFromPresentation(presentation);
    } finally {
      this.loadingSignal.set(false);
    }
  }

  private setErrorFromPresentation(presentation: ErrorPresentation): void {
    const parts: string[] = [presentation.contextMessage];

    if (presentation.detailMessage) {
      parts.push(`Motivo: ${presentation.detailMessage}`);
    }

    if (presentation.supportId) {
      parts.push(`Codigo para suporte: ${presentation.supportId}`);
      this.errorSupportIdSignal.set(presentation.supportId);
    } else {
      this.errorSupportIdSignal.set(null);
    }

    this.errorMessageSignal.set(parts.join(' '));
  }
}

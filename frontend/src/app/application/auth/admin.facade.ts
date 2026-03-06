import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import {
  ADMIN_USERS_PORT,
  AdminUser,
  AdminUsersPort,
  CreateAdminUserPayload,
  ERROR_PRESENTATION_BUILDER,
  type ErrorPresentation,
  ResetAdminUserPasswordPayload,
} from '../../domain';

function defaultErrorPresentation(_error: unknown, contextMessage: string): ErrorPresentation {
  return { contextMessage };
}

function createNoOpPort(): AdminUsersPort {
  return {
    listUsers: () => {
      throw new Error('AdminUsersPort is not configured.');
    },
    createUser: () => {
      throw new Error('AdminUsersPort is not configured.');
    },
    resetPassword: () => {
      throw new Error('AdminUsersPort is not configured.');
    },
  };
}

@Injectable({
  providedIn: 'root',
})
export class AdminFacade {
  private readonly usersSignal = signal<AdminUser[]>([]);
  private readonly loadingSignal = signal(false);
  private readonly errorMessageSignal = signal<string | null>(null);
  private readonly infoMessageSignal = signal<string | null>(null);
  private readonly searchTermSignal = signal('');

  private readonly adminUsersPort = inject(ADMIN_USERS_PORT, { optional: true }) ?? createNoOpPort();
  private readonly buildErrorPresentation =
    inject(ERROR_PRESENTATION_BUILDER, { optional: true }) ?? defaultErrorPresentation;

  private searchDebounceHandle: ReturnType<typeof setTimeout> | null = null;

  users(): AdminUser[] {
    const term = this.searchTermSignal().trim().toLowerCase();
    if (!term) {
      return this.usersSignal();
    }

    return this.usersSignal().filter((user) => user.email.toLowerCase().includes(term));
  }

  isLoading(): boolean {
    return this.loadingSignal();
  }

  errorMessage(): string | null {
    return this.errorMessageSignal();
  }

  infoMessage(): string | null {
    return this.infoMessageSignal();
  }

  searchTerm(): string {
    return this.searchTermSignal();
  }

  setSearchTerm(term: string): void {
    if (this.searchDebounceHandle) {
      clearTimeout(this.searchDebounceHandle);
    }

    this.searchDebounceHandle = setTimeout(() => {
      this.searchTermSignal.set(term);
    }, 300);
  }

  async loadUsers(): Promise<void> {
    this.loadingSignal.set(true);
    this.errorMessageSignal.set(null);

    try {
      const users = await firstValueFrom(this.adminUsersPort.listUsers());
      this.usersSignal.set(users);
    } catch (error) {
      const presentation = this.buildErrorPresentation(error, 'Nao foi possivel carregar os usuarios.');
      this.errorMessageSignal.set(this.composeErrorMessage(presentation));
    } finally {
      this.loadingSignal.set(false);
    }
  }

  async createUser(payload: CreateAdminUserPayload): Promise<void> {
    this.loadingSignal.set(true);
    this.errorMessageSignal.set(null);
    this.infoMessageSignal.set(null);

    try {
      await firstValueFrom(this.adminUsersPort.createUser(payload));
      this.infoMessageSignal.set('Usuário criado com sucesso. Senha inicial: Trocar@123');
      await this.loadUsers();
    } catch (error) {
      const presentation = this.buildErrorPresentation(error, 'Erro ao criar usuário.');
      this.errorMessageSignal.set(this.composeErrorMessage(presentation));
    } finally {
      this.loadingSignal.set(false);
    }
  }

  async resetPassword(payload: ResetAdminUserPasswordPayload): Promise<void> {
    this.loadingSignal.set(true);
    this.errorMessageSignal.set(null);
    this.infoMessageSignal.set(null);

    try {
      await firstValueFrom(this.adminUsersPort.resetPassword(payload));
      this.infoMessageSignal.set('Senha resetada com sucesso para: Trocar@123');
    } catch (error) {
      const presentation = this.buildErrorPresentation(error, 'Erro ao resetar senha.');
      this.errorMessageSignal.set(this.composeErrorMessage(presentation));
    } finally {
      this.loadingSignal.set(false);
    }
  }

  clearMessages(): void {
    this.errorMessageSignal.set(null);
    this.infoMessageSignal.set(null);
  }

  private composeErrorMessage(presentation: ErrorPresentation): string {
    const parts = [presentation.contextMessage];
    if (presentation.detailMessage) {
      parts.push(presentation.detailMessage);
    }
    if (presentation.supportId) {
      parts.push(`(${presentation.supportId})`);
    }
    return parts.join(' ');
  }
}

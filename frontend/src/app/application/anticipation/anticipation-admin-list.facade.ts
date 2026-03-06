import { inject, Injectable, signal } from '@angular/core';
import {
  ANTICIPATION_REQUESTS_PORT,
  AnticipationAdminListFilter,
  AnticipationRequest,
  ERROR_PRESENTATION_BUILDER,
  type ErrorPresentation,
} from '../../domain';
import { firstValueFrom } from 'rxjs';

/** Minimal fallback when ERROR_PRESENTATION_BUILDER is not provided (e.g. in tests). */
function defaultErrorPresentation(_error: unknown, contextMessage: string): ErrorPresentation {
  return { contextMessage };
}

@Injectable({
  providedIn: 'root',
})
export class AnticipationAdminListFacade {
  private readonly _requests = signal<AnticipationRequest[]>([]);
  private readonly _totalCount = signal(0);
  private readonly _filters = signal<AnticipationAdminListFilter | undefined>(undefined);
  private readonly _loading = signal(false);
  private readonly _errorMessage = signal<string | null>(null);
  private readonly _infoMessage = signal<string | null>(null);
  private readonly _errorSupportId = signal<string | null>(null);
  private readonly _selectedRequest = signal<AnticipationRequest | null>(null);

  readonly requests = this._requests.asReadonly();
  readonly totalCount = this._totalCount.asReadonly();
  readonly filters = this._filters.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly errorMessage = this._errorMessage.asReadonly();
  readonly infoMessage = this._infoMessage.asReadonly();
  readonly errorSupportId = this._errorSupportId.asReadonly();
  readonly selectedRequest = this._selectedRequest.asReadonly();

  private readonly port = inject(ANTICIPATION_REQUESTS_PORT);
  private readonly buildErrorPresentation =
    inject(ERROR_PRESENTATION_BUILDER, { optional: true }) ?? defaultErrorPresentation;

  async loadInitial(): Promise<void> {
    const now = new Date();
    const from = new Date(now);
    from.setDate(from.getDate() - 7);

    const filter: AnticipationAdminListFilter = {
      period: { from, to: now },
      page: 1,
      pageSize: 20,
    };

    this._filters.set(filter);
    await this.loadWithFilter(filter);
  }

  async applyFilters(filter: AnticipationAdminListFilter): Promise<void> {
    this._filters.set(filter);
    await this.loadWithFilter(filter);
  }

  clearFilters(): Promise<void> {
    this._filters.set(undefined);
    return this.loadInitial();
  }

  async setPage(page: number): Promise<void> {
    const current = this._filters();
    if (!current) return;
    const next: AnticipationAdminListFilter = { ...current, page };
    this._filters.set(next);
    await this.loadWithFilter(next);
  }

  async selectRequest(id: string): Promise<void> {
    this._loading.set(true);
    this._errorMessage.set(null);
    this._infoMessage.set(null);
    this._errorSupportId.set(null);

    try {
      const request = await firstValueFrom(this.port.getRequestDetail(id));
      this._selectedRequest.set(request);
    } catch (error) {
      const presentation = this.buildErrorPresentation(
        error,
        'Nao foi possivel carregar os detalhes da solicitacao.',
      );
      this.setErrorFromPresentation(presentation);
    } finally {
      this._loading.set(false);
    }
  }

  async approveRequest(id: string, observation?: string): Promise<void> {
    this._errorMessage.set(null);
    this._infoMessage.set(null);
    this._errorSupportId.set(null);

    try {
      const updated = await firstValueFrom(this.port.approveRequest(id, observation));
      this._selectedRequest.set(updated);
      this._requests.update((list) =>
        list.map((r) => (r.id === id ? updated : r)),
      );
      this._infoMessage.set('Solicitação aprovada.');
    } catch (error) {
      const presentation = this.buildErrorPresentation(
        error,
        'Nao foi possivel aprovar a solicitacao.',
      );
      this.setErrorFromPresentation(presentation);
    }
  }

  async rejectRequest(id: string, reason: string): Promise<void> {
    this._errorMessage.set(null);
    this._infoMessage.set(null);
    this._errorSupportId.set(null);

    try {
      const updated = await firstValueFrom(this.port.rejectRequest(id, reason));
      this._selectedRequest.set(updated);
      this._requests.update((list) =>
        list.map((r) => (r.id === id ? updated : r)),
      );
      this._infoMessage.set('Solicitação recusada.');
    } catch (error) {
      const presentation = this.buildErrorPresentation(
        error,
        'Nao foi possivel recusar a solicitacao.',
      );
      this.setErrorFromPresentation(presentation);
    }
  }

  clearMessages(): void {
    this._errorMessage.set(null);
    this._infoMessage.set(null);
    this._errorSupportId.set(null);
  }

  private async loadWithFilter(filter: AnticipationAdminListFilter): Promise<void> {
    this._loading.set(true);
    this._errorMessage.set(null);
    this._infoMessage.set(null);
    this._errorSupportId.set(null);

    try {
      const result = await firstValueFrom(this.port.listGlobalRequests(filter));
      this._requests.set(result.items);
      this._totalCount.set(result.totalCount);
    } catch (error) {
      const presentation = this.buildErrorPresentation(
        error,
        'Nao foi possivel carregar a lista de solicitacoes.',
      );
      this.setErrorFromPresentation(presentation);
    } finally {
      this._loading.set(false);
    }
  }

  private setErrorFromPresentation(presentation: {
    contextMessage: string;
    detailMessage?: string;
    supportId?: string;
  }): void {
    const parts: string[] = [presentation.contextMessage];
    if (presentation.detailMessage) {
      parts.push(`Motivo: ${presentation.detailMessage}`);
    }
    if (presentation.supportId) {
      parts.push(`Codigo para suporte: ${presentation.supportId}`);
      this._errorSupportId.set(presentation.supportId);
    } else {
      this._errorSupportId.set(null);
    }
    this._errorMessage.set(parts.join(' '));
  }
}

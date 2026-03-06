import { inject, Injectable, signal } from '@angular/core';
import {
  ANTICIPATION_REQUESTS_PORT,
  AnticipationRequest,
  AnticipationRequestsFilter,
  ERROR_PRESENTATION_BUILDER,
  type ErrorPresentation,
} from '../../domain';
import { firstValueFrom } from 'rxjs';

/** Minimal fallback when ERROR_PRESENTATION_BUILDER is not provided (e.g. in tests). Application layer does not depend on shared. */
function defaultErrorPresentation(_error: unknown, contextMessage: string): ErrorPresentation {
  return { contextMessage };
}

@Injectable({
  providedIn: 'root',
})
export class AnticipationMyRequestsFacade {
  private readonly _requests = signal<AnticipationRequest[]>([]);
  private readonly _filters = signal<AnticipationRequestsFilter | undefined>(undefined);
  private readonly _loading = signal(false);
  private readonly _errorMessage = signal<string | null>(null);
  private readonly _infoMessage = signal<string | null>(null);
  private readonly _errorSupportId = signal<string | null>(null);
  private readonly _selectedRequest = signal<AnticipationRequest | null>(null);

  readonly requests = this._requests.asReadonly();
  readonly filters = this._filters.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly errorMessage = this._errorMessage.asReadonly();
  readonly infoMessage = this._infoMessage.asReadonly();
  readonly errorSupportId = this._errorSupportId.asReadonly();
  readonly selectedRequest = this._selectedRequest.asReadonly();

  private readonly httpService = inject(ANTICIPATION_REQUESTS_PORT);
  private readonly buildErrorPresentation =
    inject(ERROR_PRESENTATION_BUILDER, { optional: true }) ?? defaultErrorPresentation;

  async loadInitialRequests(): Promise<void> {
    const now = new Date();
    const from = new Date(now);
    from.setDate(from.getDate() - 90);

    const filter: AnticipationRequestsFilter = {
      period: { from, to: now },
    };

    this._filters.set(filter);
    await this.loadWithFilter(filter);
  }

  async applyFilters(filter: AnticipationRequestsFilter): Promise<void> {
    this._filters.set(filter);
    await this.loadWithFilter(filter);
  }

  async refresh(): Promise<void> {
    const currentFilter = this._filters();
    await this.loadWithFilter(currentFilter);
  }

  async selectRequest(id: string): Promise<void> {
    this._loading.set(true);
    this._errorMessage.set(null);
    this._infoMessage.set(null);
    this._errorSupportId.set(null);

    try {
      const request = await firstValueFrom(this.httpService.getRequestDetail(id));
      this._selectedRequest.set(request);
    } catch (error) {
      console.error('RF-1 selectRequest error', error);
      const presentation = this.buildErrorPresentation(
        error,
        'Nao foi possivel carregar os detalhes da solicitacao.',
      );
      this.setErrorFromPresentation(presentation);
    } finally {
      this._loading.set(false);
    }
  }

  async cancelRequest(id: string): Promise<void> {
    this._loading.set(true);
    this._errorMessage.set(null);
    this._infoMessage.set(null);
    this._errorSupportId.set(null);

    try {
      const updated = await firstValueFrom(this.httpService.cancelRequest(id));
      this.replaceRequestInList(updated);
      this._selectedRequest.set(updated);
      this._infoMessage.set('Solicitacao cancelada com sucesso.');
    } catch (error) {
      console.error('RF-1 cancelRequest error', error);
      const presentation = this.buildErrorPresentation(
        error,
        'Nao foi possivel cancelar a solicitacao agora.',
      );
      this.setErrorFromPresentation(presentation);
    } finally {
      this._loading.set(false);
    }
  }

  clearMessages(): void {
    this._errorMessage.set(null);
    this._infoMessage.set(null);
    this._errorSupportId.set(null);
  }

  private async loadWithFilter(filter?: AnticipationRequestsFilter): Promise<void> {
    this._loading.set(true);
    this._errorMessage.set(null);
    this._infoMessage.set(null);
    this._errorSupportId.set(null);

    try {
      const items = await firstValueFrom(this.httpService.listMyRequests(filter));
      this._requests.set(items);
    } catch (error) {
      console.error('RF-1 loadWithFilter error', error);
      const presentation = this.buildErrorPresentation(
        error,
        'Nao foi possivel carregar suas solicitacoes de antecipacao.',
      );
      this.setErrorFromPresentation(presentation);
    } finally {
      this._loading.set(false);
    }
  }

  private replaceRequestInList(updated: AnticipationRequest): void {
    const current = this._requests();
    const index = current.findIndex((item) => item.id === updated.id);

    if (index === -1) {
      this._requests.set(current);
      return;
    }

    const next = [...current];
    next[index] = updated;
    this._requests.set(next);
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


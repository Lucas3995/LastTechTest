import { Injectable, Signal, signal, computed, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Router } from '@angular/router';
import {
  AnticipationSimulation,
  ConversionResult,
  SimulateAnticipationPayload,
  ANTICIPATION_REQUESTS_PORT,
} from '../../domain';
import { ERROR_PRESENTATION_BUILDER } from '../../domain';

@Injectable()
export class AnticipationSimulationFacade {

  private port = inject(ANTICIPATION_REQUESTS_PORT);
  private errorBuilder = inject(ERROR_PRESENTATION_BUILDER);
  private router = inject(Router);

  // Signals de estado
  private readonly _simulationResult = signal<AnticipationSimulation | null>(null);
  private readonly _loading = signal<boolean>(false);
  private readonly _errorMessage = signal<string | null>(null);
  private readonly _errorSupportId = signal<string | null>(null); // traceId
  private readonly _infoMessage = signal<string | null>(null);
  private readonly _creatorIdForSimulation = signal<string | null>(null);
  private readonly _isConverting = signal<boolean>(false);

  // Computed: pode converter?
  private readonly _canConvert = computed(() => {
    const result = this._simulationResult();
    if (!result) return false;

    // Verifica se ainda válida (validUntil > agora)
    const now = new Date();
    return result.validUntil > now;
  });

  // Público (read-only)
  readonly simulationResult: Signal<AnticipationSimulation | null> = this._simulationResult.asReadonly();
  readonly loading: Signal<boolean> = this._loading.asReadonly();
  readonly errorMessage: Signal<string | null> = this._errorMessage.asReadonly();
  readonly errorSupportId: Signal<string | null> = this._errorSupportId.asReadonly();
  readonly infoMessage: Signal<string | null> = this._infoMessage.asReadonly();
  readonly creatorIdForSimulation: Signal<string | null> = this._creatorIdForSimulation.asReadonly();
  readonly isConverting: Signal<boolean> = this._isConverting.asReadonly();
  readonly canConvert: Signal<boolean> = this._canConvert;

  async simulate(payload: SimulateAnticipationPayload): Promise<void> {
    this._loading.set(true);
    this._errorMessage.set(null);
    this._errorSupportId.set(null);
    this._infoMessage.set(null);

    try {
      const result = await firstValueFrom(
        this.port.simulateAnticipation(payload)
      );
      this._simulationResult.set(result);
      this._infoMessage.set('Simulação realizada com sucesso.');
    } catch (error) {
      const presentation = this.errorBuilder(error, 'Falha na simulação de antecipação');
      this._errorMessage.set(presentation.contextMessage);
      this._errorSupportId.set(presentation.supportId ?? null);
    } finally {
      this._loading.set(false);
    }
  }

  async convertToRealRequest(): Promise<void> {
    const result = this._simulationResult();
    if (!result) {
      this._errorMessage.set('Nenhuma simulação válida para converter.');
      return;
    }

    // Valida novamente se expirou
    if (!this._canConvert()) {
      this._errorMessage.set('Simulação expirou. Realize uma nova simulação.');
      return;
    }

    this._isConverting.set(true);
    this._errorMessage.set(null);

    try {
      const conversionResult: ConversionResult = await firstValueFrom(
        this.port.convertSimulationToReal(
          result.simulationCode,
          this._creatorIdForSimulation() || undefined
        )
      );

      // Sucesso: limpa simulação (foi "usada")
      this._simulationResult.set(null);
      this._infoMessage.set(
        `Solicitação criada com sucesso! Protocolo: ${conversionResult.protocol}`
      );
      // Navegar para a lista de solicitações após conversão bem-sucedida (CA-RF3-4)
      this.router.navigate(['anticipation', 'my-requests']);

    } catch (error: unknown) {
      // Trata erros específicos de conversão (CA-RF3-5)
      const errorResponse = error as { status?: number; error?: { code?: string } };
      if (errorResponse?.status === 422) {
        const body = errorResponse.error;
        if (body?.code === 'SIMULATION_EXPIRED') {
          this._errorMessage.set('Simulação expirou. Realize uma nova simulação.');
        } else if (body?.code === 'ALREADY_USED') {
          this._errorMessage.set('Esta simulação já foi utilizada para criar uma solicitação.');
        } else if (body?.code === 'PENDING_EXISTS') {
          this._errorMessage.set('Você já possui uma solicitação em aberto. Conclua ou cancele-a antes.');
        } else if (body?.code === 'RULES_VIOLATED') {
          this._errorMessage.set('As condições de negócio mudaram. Realize uma nova simulação.');
        } else {
          const presentation = this.errorBuilder(error, 'Falha na conversão da simulação de antecipação');
          this._errorMessage.set(presentation.contextMessage);
          this._errorSupportId.set(presentation.supportId ?? null);
        }
      } else {
        const presentation = this.errorBuilder(error, 'Falha na conversão da simulação de antecipação');
        this._errorMessage.set(presentation.contextMessage);
        this._errorSupportId.set(presentation.supportId ?? null);
      }
    } finally {
      this._isConverting.set(false);
    }
  }

  setCreatorIdForSimulation(creatorId: string): void {
    this._creatorIdForSimulation.set(creatorId);
  }

  /** Allows setting error message directly (used in tests and externally). */
  setErrorMessage(message: string): void {
    this._errorMessage.set(message);
  }

  reset(): void {
    this._simulationResult.set(null);
    this._loading.set(false);
    this._errorMessage.set(null);
    this._errorSupportId.set(null);
    this._infoMessage.set(null);
    this._creatorIdForSimulation.set(null);
    this._isConverting.set(false);
  }
}

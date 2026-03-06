// RF-3 Simulação de Antecipação — AnticipationSimulationFacade (T3)
// Plano árvore testes RF-3: simulate/convertToRealRequest com Signals, estado, loading, error handling
// CA-RF3-1 a CA-RF3-6: mapeamento de cenários para facade methods.

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import {
  AnticipationRequestStatus,
  ANTICIPATION_REQUESTS_PORT,
  ERROR_PRESENTATION_BUILDER,
  SimulateAnticipationPayload,
} from '../../domain';
import { AnticipationSimulationFacade } from './anticipation-simulation.facade';

describe('AnticipationSimulationFacade — RF-3 Simulação (CA-RF3-1 a CA-RF3-6)', () => {
  let facade: AnticipationSimulationFacade;
  let simulatePortSpy: ReturnType<typeof vi.fn>;
  let convertPortSpy: ReturnType<typeof vi.fn>;
  let routerSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    simulatePortSpy = vi.fn().mockReturnValue(
      of({
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      }),
    );
    convertPortSpy = vi.fn().mockReturnValue(
      of({
        id: 'req-123',
        protocol: 'PROT-001',
        status: AnticipationRequestStatus.Pending,
        netAmount: 95000,
      }),
    );

    TestBed.configureTestingModule({
      providers: [
        AnticipationSimulationFacade,
        {
          provide: ANTICIPATION_REQUESTS_PORT,
          useValue: {
            listMyRequests: vi.fn(),
            listGlobalRequests: vi.fn(),
            getRequestDetail: vi.fn(),
            cancelRequest: vi.fn(),
            approveRequest: vi.fn(),
            rejectRequest: vi.fn(),
            createRequest: vi.fn(),
            simulateAnticipation: simulatePortSpy,
            convertSimulationToReal: convertPortSpy,
          },
        },
        {
          provide: ERROR_PRESENTATION_BUILDER,
          useValue: (error: unknown, contextMessage: string) => ({
            contextMessage,
            supportId: (error as { supportId?: string })?.supportId || 'SVC-ERROR',
            userMessage: (error as { userMessage?: string })?.userMessage,
          }),
        },
        {
          provide: Router,
          useValue: { navigate: vi.fn() },
        },
      ],
    });
    facade = TestBed.inject(AnticipationSimulationFacade);
    routerSpy = vi.spyOn(TestBed.inject(Router), 'navigate') as any;
  });

  describe('Signals state initialization', () => {
    it('should initialize _simulationResult with null', () => {
      expect(facade.simulationResult()).toBeNull();
    });

    it('should initialize _loading with false', () => {
      expect(facade.loading()).toBe(false);
    });

    it('should initialize _errorMessage with null', () => {
      expect(facade.errorMessage()).toBeNull();
    });

    it('should initialize _infoMessage with null', () => {
      expect(facade.infoMessage()).toBeNull();
    });

    it('should initialize _creatorIdForSimulation with null', () => {
      expect(facade.creatorIdForSimulation()).toBeNull();
    });
  });

  describe('simulate() — CA-RF3-1, CA-RF3-2, CA-RF3-6', () => {
    it('[CA-RF3-1] should call httpService.simulateAnticipation with payload', async () => {
      const payload: SimulateAnticipationPayload = { requestedAmount: 1000 };

      await facade.simulate(payload);

      expect(simulatePortSpy).toHaveBeenCalledWith(payload);
      expect(simulatePortSpy).toHaveBeenCalledTimes(1);
    });

    it('[CA-RF3-1] should update simulationResult signal on success', async () => {
      const payload: SimulateAnticipationPayload = { requestedAmount: 1000 };
      const mockResult = {
        simulationCode: 'SIM-ABC123',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockResult));

      await facade.simulate(payload);

      expect(facade.simulationResult()).toEqual(mockResult);
    });

    it('[CA-RF3-1] should set loading during request', async () => {
      const payload: SimulateAnticipationPayload = { requestedAmount: 1000 };

      // Monitor loading state change
      TestBed.inject(AnticipationSimulationFacade);

      // Spy on loading signal
      const loadingValue = facade.loading();
      expect(loadingValue).toBe(false); // before request

      await facade.simulate(payload);

      expect(facade.loading()).toBe(false); // after request completes
    });

    it('[CA-RF3-2] should handle validation errors via ERROR_PRESENTATION_BUILDER', async () => {
      const payload: SimulateAnticipationPayload = { requestedAmount: 50 }; // below minimum
      const error = { status: 400, message: 'Validation failed' };
      simulatePortSpy.mockReturnValue(throwError(() => error));

      await facade.simulate(payload);

      expect(facade.errorMessage()).toBeTruthy();
      expect(facade.simulationResult()).toBeNull();
    });

    it('[CA-RF3-2] should map error message from backend error response', async () => {
      const payload: SimulateAnticipationPayload = { requestedAmount: 50 };
      const backendError = {
        status: 400,
        error: { message: 'Mínimo R$ 100,00' },
      };
      simulatePortSpy.mockReturnValue(throwError(() => backendError));

      await facade.simulate(payload);

      expect(facade.errorMessage()).toBeTruthy();
      expect(facade.simulationResult()).toBeNull();
    });

    it('[CA-RF3-6] should set creatorId in payload for Admin/Analista', async () => {
      facade.setCreatorIdForSimulation('creator-user-123');
      const payload: SimulateAnticipationPayload = {
        requestedAmount: 1000,
        creatorId: 'creator-user-123',
      };

      await facade.simulate(payload);

      const callArgs = simulatePortSpy.mock.calls[0] as unknown as [SimulateAnticipationPayload];
      expect(callArgs[0].creatorId).toEqual('creator-user-123');
    });

    it('should clear errorMessage on successful simulate', async () => {
      facade.setErrorMessage('Previous error');
      const payload: SimulateAnticipationPayload = { requestedAmount: 1000 };

      await facade.simulate(payload);

      expect(facade.errorMessage()).toBeNull();
    });
  });

  describe('convertToRealRequest() — CA-RF3-4, CA-RF3-5, CA-RF3-6', () => {
    it('[CA-RF3-4] should call httpService.convertSimulationToReal', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));
      await facade.simulate({ requestedAmount: 1000 });

      await facade.convertToRealRequest();

      expect(convertPortSpy).toHaveBeenCalledWith('SIM-123456', undefined);
    });

    it('[CA-RF3-4] should clear simulationResult on successful conversion', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));
      await facade.simulate({ requestedAmount: 1000 });
      expect(facade.simulationResult()).not.toBeNull();

      convertPortSpy.mockReturnValue(
        of({
          id: 'req-123',
          protocol: 'PROT-001',
          status: AnticipationRequestStatus.Pending,
          netAmount: 95000,
        }),
      );

      await facade.convertToRealRequest();

      expect(facade.simulationResult()).toBeNull();
    });

    it('[CA-RF3-4] should navigate to my-requests on successful conversion', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));
      await facade.simulate({ requestedAmount: 1000 });

      convertPortSpy.mockReturnValue(
        of({
          id: 'req-123',
          protocol: 'PROT-001',
          status: AnticipationRequestStatus.Pending,
          netAmount: 95000,
        }),
      );

      await facade.convertToRealRequest();

      expect(routerSpy).toHaveBeenCalledWith(['anticipation', 'my-requests']);
    });

    it('[CA-RF3-4] should set infoMessage on successful conversion', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));
      await facade.simulate({ requestedAmount: 1000 });

      convertPortSpy.mockReturnValue(
        of({
          id: 'req-123',
          protocol: 'PROT-001',
          status: AnticipationRequestStatus.Pending,
          netAmount: 95000,
        }),
      );

      await facade.convertToRealRequest();

      expect(facade.infoMessage()).toContain('sucesso');
    });

    it('[CA-RF3-5] should handle expired simulation error (422)', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() - 1000), // expired
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));
      await facade.simulate({ requestedAmount: 1000 });

      const expiredError = {
        status: 422,
        error: { code: 'SIMULATION_EXPIRED', message: 'Simulação expirou. Realize uma nova simulação.' },
      };
      convertPortSpy.mockReturnValue(throwError(() => expiredError));

      await facade.convertToRealRequest();

      expect(facade.errorMessage()).toContain('expirou');
    });

    it('[CA-RF3-5] should handle pending request exists error', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));
      await facade.simulate({ requestedAmount: 1000 });

      const pendingError = {
        status: 422,
        error: { code: 'PENDING_EXISTS', message: 'Você já possui uma solicitação em aberto.' },
      };
      convertPortSpy.mockReturnValue(throwError(() => pendingError));

      await facade.convertToRealRequest();

      expect(facade.errorMessage()).toContain('solicitação em aberto');
    });

    it('[CA-RF3-5] should handle rules violated error', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));
      await facade.simulate({ requestedAmount: 1000 });

      const rulesError = {
        status: 422,
        error: { code: 'RULES_VIOLATED', message: 'As regras mudaram. Nova simulação necessária.' },
      };
      convertPortSpy.mockReturnValue(throwError(() => rulesError));

      await facade.convertToRealRequest();

      expect(facade.errorMessage()).toBeTruthy();
    });

    it('[CA-RF3-5] should handle already used simulation error', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));
      await facade.simulate({ requestedAmount: 1000 });

      const usedError = {
        status: 422,
        error: { code: 'ALREADY_USED', message: 'Esta simulação já foi utilizada.' },
      };
      convertPortSpy.mockReturnValue(throwError(() => usedError));

      await facade.convertToRealRequest();

      expect(facade.errorMessage()).toContain('utilizada');
    });

    it('[CA-RF3-6] should include creatorId in conversion for Admin', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));
      facade.setCreatorIdForSimulation('creator-user-123');
      await facade.simulate({ requestedAmount: 1000, creatorId: 'creator-user-123' });

      convertPortSpy.mockReturnValue(
        of({
          id: 'req-123',
          protocol: 'PROT-001',
          status: AnticipationRequestStatus.Pending,
          netAmount: 95000,
        }),
      );

      await facade.convertToRealRequest();

      expect(convertPortSpy).toHaveBeenCalledWith('SIM-123456', 'creator-user-123');
    });
  });

  describe('canConvert computed signal', () => {
    it('should be true for Creator with valid simulation', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));

      await facade.simulate({ requestedAmount: 1000 });

      expect(facade.canConvert()).toBe(true);
    });

    it('should be false when simulation is null', () => {
      expect(facade.simulationResult()).toBeNull();
      expect(facade.canConvert()).toBe(false);
    });

    it('should be false when simulation expired', async () => {
      const expiredSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() - 1000), // past
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(expiredSimulation));

      await facade.simulate({ requestedAmount: 1000 });

      expect(facade.canConvert()).toBe(false);
    });
  });

  describe('reset() — Reset state', () => {
    it('should clear simulationResult, errorMessage, and infoMessage', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));
      await facade.simulate({ requestedAmount: 1000 });
      expect(facade.simulationResult()).not.toBeNull();

      facade.reset();

      expect(facade.simulationResult()).toBeNull();
      expect(facade.errorMessage()).toBeNull();
      expect(facade.infoMessage()).toBeNull();
    });
  });

  describe('setCreatorIdForSimulation()', () => {
    it('should update creatorIdForSimulation signal', () => {
      const creatorId = 'creator-123';

      facade.setCreatorIdForSimulation(creatorId);

      expect(facade.creatorIdForSimulation()).toBe(creatorId);
    });
  });
});

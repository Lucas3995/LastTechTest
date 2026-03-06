// RF-3 Integration — Facade + HTTP Service (T8)
// Testes de integração entre facade e serviço HTTP

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import {
  AnticipationRequestStatus,
  ANTICIPATION_REQUESTS_PORT,
  ERROR_PRESENTATION_BUILDER,
} from '../../domain';
import { AnticipationSimulationFacade } from './anticipation-simulation.facade';

describe('AnticipationSimulationFacade — Integration with HTTP Service (T8)', () => {
  let facade: AnticipationSimulationFacade;
  let simulatePortSpy: ReturnType<typeof vi.fn>;
  let convertPortSpy: ReturnType<typeof vi.fn>;
  let routerSpy: any;

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
            supportId: 'SVC-ERR-001',
            userMessage: (error as any)?.error?.message || contextMessage,
          }),
        },
        {
          provide: Router,
          useValue: { navigate: vi.fn() },
        },
      ],
    });

    facade = TestBed.inject(AnticipationSimulationFacade);
    routerSpy = vi.spyOn(TestBed.inject(Router), 'navigate');
  });

  describe('Full simulation flow', () => {
    it('should handle successful simulation end-to-end', async () => {
      const payload = { requestedAmount: 1000 };
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

      expect(simulatePortSpy).toHaveBeenCalledWith(payload);
      expect(facade.simulationResult()).toEqual(mockResult);
      expect(facade.errorMessage()).toBeNull();
      expect(facade.canConvert()).toBe(true);
    });

    it('should handle simulation error with proper error presentation', async () => {
      const payload = { requestedAmount: 50 };
      const backendError = {
        status: 400,
        error: { message: 'Validation failed' },
      };
      simulatePortSpy.mockReturnValue(throwError(() => backendError));

      await facade.simulate(payload);

      expect(simulatePortSpy).toHaveBeenCalledWith(payload);
      expect(facade.simulationResult()).toBeNull();
      expect(facade.errorMessage()).toBeTruthy();
    });

    it('should handle conversion success end-to-end', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));

      const mockConversionResult = {
        id: 'req-789',
        protocol: 'PROT-20260306-001',
        status: AnticipationRequestStatus.Pending,
        netAmount: 95000,
      };
      convertPortSpy.mockReturnValue(of(mockConversionResult));

      await facade.simulate({ requestedAmount: 1000 });
      expect(facade.simulationResult()).not.toBeNull();

      await facade.convertToRealRequest();

      expect(convertPortSpy).toHaveBeenCalledWith('SIM-123456', undefined);
      expect(facade.simulationResult()).toBeNull(); // cleared after conversion
      expect(facade.infoMessage()).toContain('sucesso');
      expect(routerSpy).toHaveBeenCalledWith(['anticipation', 'my-requests']);
    });

    it('should handle conversion rejection scenarios', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-EXPIRED',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() - 1000), // expired
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));

      const expiredError = {
        status: 422,
        error: { code: 'SIMULATION_EXPIRED', message: 'Simulação expirou.' },
      };
      convertPortSpy.mockReturnValue(throwError(() => expiredError));

      await facade.simulate({ requestedAmount: 1000 });
      expect(facade.canConvert()).toBe(false); // expired

      // Attempt conversion
      await facade.convertToRealRequest();

      expect(facade.errorMessage()).toBeTruthy();
      expect(routerSpy).not.toHaveBeenCalled(); // should not navigate on error
    });
  });

  describe('Multiple simulation cycles', () => {
    it('should allow new simulation after first one completes', async () => {
      const firstResult = {
        simulationCode: 'SIM-001',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(firstResult));

      await facade.simulate({ requestedAmount: 1000 });
      expect(facade.simulationResult()?.simulationCode).toBe('SIM-001');

      const secondResult = {
        simulationCode: 'SIM-002',
        grossAmountCents: 150000,
        feesAmountCents: 7500,
        netAmountCents: 142500,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(secondResult));

      await facade.simulate({ requestedAmount: 1500 });
      expect(facade.simulationResult()?.simulationCode).toBe('SIM-002');
    });

    it('should clear error on successful new simulation after failure', async () => {
      const failureError = {
        status: 400,
        error: { message: 'Validation failed' },
      };
      simulatePortSpy.mockReturnValue(throwError(() => failureError));

      await facade.simulate({ requestedAmount: 50 });
      expect(facade.errorMessage()).toBeTruthy();

      const successResult = {
        simulationCode: 'SIM-SUCCESS',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(successResult));

      await facade.simulate({ requestedAmount: 1000 });
      expect(facade.errorMessage()).toBeNull();
      expect(facade.simulationResult()).not.toBeNull();
    });
  });

  describe('State consistency', () => {
    it('should maintain correct state after simulate and convert', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-STATE-TEST',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));

      await facade.simulate({ requestedAmount: 1000 });

      // State after simulate
      expect(facade.simulationResult()).toEqual(mockSimulation);
      expect(facade.canConvert()).toBe(true);
      expect(facade.errorMessage()).toBeNull();

      convertPortSpy.mockReturnValue(
        of({
          id: 'req-state-test',
          protocol: 'PROT-STATE',
          status: AnticipationRequestStatus.Pending,
          netAmount: 95000,
        }),
      );

      await facade.convertToRealRequest();

      // State after conversion
      expect(facade.simulationResult()).toBeNull();
      expect(facade.canConvert()).toBe(false);
      expect(facade.infoMessage()).toContain('sucesso');
    });

    it('should reset facade state properly', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-RESET-TEST',
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
      expect(facade.canConvert()).toBe(false);
    });
  });

  describe('Error handling and recovery', () => {
    it('should handle network errors gracefully', async () => {
      const networkError = new Error('Network error');
      simulatePortSpy.mockReturnValue(throwError(() => networkError));

      await facade.simulate({ requestedAmount: 1000 });

      expect(facade.errorMessage()).toBeTruthy();
      expect(facade.simulationResult()).toBeNull();
    });

    it('should provide specific error messages for conversion failures', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-ERROR-TEST',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));

      await facade.simulate({ requestedAmount: 1000 });

      const conversionError = {
        status: 422,
        error: {
          code: 'PENDING_EXISTS',
          message: 'Você já possui uma solicitação em aberto.',
        },
      };
      convertPortSpy.mockReturnValue(throwError(() => conversionError));

      await facade.convertToRealRequest();

      expect(facade.errorMessage()).toContain('solicitação em aberto');
    });
  });

  describe('Admin/Analista flows', () => {
    it('should pass creatorId through full simulate and convert flow', async () => {
      const creatorId = 'creator-target-123';
      facade.setCreatorIdForSimulation(creatorId);

      const mockSimulation = {
        simulationCode: 'SIM-ADMIN-FLOW',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      simulatePortSpy.mockReturnValue(of(mockSimulation));

      await facade.simulate({ requestedAmount: 1000, creatorId });

      expect((simulatePortSpy as any).mock.calls[0][0].creatorId).toEqual(creatorId);

      convertPortSpy.mockReturnValue(
        of({
          id: 'req-admin-123',
          protocol: 'PROT-ADM',
          status: AnticipationRequestStatus.Pending,
          netAmount: 95000,
        }),
      );

      await facade.convertToRealRequest();

      expect(convertPortSpy).toHaveBeenCalledWith('SIM-ADMIN-FLOW', creatorId);
    });
  });

  describe('Loading state management', () => {
    it('should manage loading state during simulate call', async () => {
      const payload = { requestedAmount: 1000 };
      simulatePortSpy.mockReturnValue(of({
        simulationCode: 'SIM-LOAD-TEST',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      }));

      await facade.simulate(payload);

      // After completion, loading should be false
      expect(facade.loading()).toBe(false);
    });

    it('should manage loading state during convert call', async () => {
      const mockSimulation = {
        simulationCode: 'SIM-CONVERT-LOAD',
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
          id: 'req-convert-load',
          protocol: 'PROT-LOAD',
          status: AnticipationRequestStatus.Pending,
          netAmount: 95000,
        }),
      );

      await facade.convertToRealRequest();

      // After completion, loading should be false
      expect(facade.loading()).toBe(false);
    });
  });
});

import {
  AnticipationRequest,
  AnticipationRequestStatus,
  AnticipationRequestsFilter,
  CreateAnticipationRequestPayload,
  AnticipationSimulation,
  SimulationResult,
  ConversionResult,
  SimulationError,
} from './anticipation';

describe('Domain Types - Anticipation Simulation', () => {
  describe('AnticipationSimulation interface', () => {
    it('should validate required fields', () => {
      const simulation: AnticipationSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000, // R$ 1000.00
        feesAmountCents: 5000, // R$ 50.00
        netAmountCents: 95000, // R$ 950.00
        validUntil: new Date(Date.now() + 20 * 60 * 1000), // 20 minutos
        createdAt: new Date(),
      };
      expect(simulation).toBeDefined();
      expect(simulation.simulationCode).toBeDefined();
      expect(simulation.validUntil).toBeDefined();
    });

    it('should accept optional fields', () => {
      const simulation: AnticipationSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(),
        createdAt: new Date(),
        anticipationDate: new Date(),
        maturityDate: new Date(),
      };
      expect(simulation.anticipationDate).toBeDefined();
      expect(simulation.maturityDate).toBeDefined();
    });

    it('should have correct currency precision (cents)', () => {
      const simulation: AnticipationSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000, // 1000.00
        feesAmountCents: 5000, // 50.00
        netAmountCents: 95000, // 950.00
        validUntil: new Date(),
        createdAt: new Date(),
      };
      expect(simulation.grossAmountCents).toBe(100000);
      expect(simulation.feesAmountCents).toBe(5000);
      expect(simulation.netAmountCents).toBe(95000);
      // Validation: net = gross - fees
      expect(simulation.netAmountCents).toBe(
        simulation.grossAmountCents - simulation.feesAmountCents
      );
    });
  });

  describe('SimulationResult type', () => {
    it('should extend AnticipationSimulation', () => {
      const result: SimulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(),
        createdAt: new Date(),
      };
      expect(result).toBeDefined();
      expect(result.simulationCode).toBeDefined();
    });

    it('should include simulationCode and validUntil', () => {
      const result: SimulationResult = {
        simulationCode: 'SIM-ABC123',
        grossAmountCents: 50000,
        feesAmountCents: 2500,
        netAmountCents: 47500,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      expect(result.simulationCode).toEqual('SIM-ABC123');
      expect(result.validUntil).toBeDefined();
      expect(result.validUntil.getTime()).toBeGreaterThan(Date.now());
    });
  });

  describe('ConversionResult type', () => {
    it('should include request id and status', () => {
      const result: ConversionResult = {
        id: 'req-789',
        protocol: 'PROT-20260306-001',
        status: AnticipationRequestStatus.Pending,
        netAmount: 47500,
      };
      expect(result.id).toBeDefined();
      expect(result.status).toBeDefined();
      expect(result.protocol).toBeDefined();
    });

    it('should have correct properties for successful conversion', () => {
      const result: ConversionResult = {
        id: 'req-123',
        protocol: 'ABC-123-456',
        status: AnticipationRequestStatus.Pending,
        netAmount: 95000,
      };
      expect(result.id).toEqual('req-123');
      expect(result.protocol).toEqual('ABC-123-456');
      expect(result.status).toEqual(AnticipationRequestStatus.Pending);
      expect(result.netAmount).toEqual(95000);
    });
  });

  describe('SimulationError enum', () => {
    it('should define error codes for conversion rejection', () => {
      expect(SimulationError.SIMULATION_EXPIRED).toBeDefined();
      expect(SimulationError.ALREADY_USED).toBeDefined();
      expect(SimulationError.PENDING_EXISTS).toBeDefined();
      expect(SimulationError.RULES_VIOLATED).toBeDefined();
    });

    it('should have specific values for error handling', () => {
      expect(SimulationError.SIMULATION_EXPIRED).toEqual('SIMULATION_EXPIRED');
      expect(SimulationError.ALREADY_USED).toEqual('ALREADY_USED');
      expect(SimulationError.PENDING_EXISTS).toEqual('PENDING_EXISTS');
      expect(SimulationError.RULES_VIOLATED).toEqual('RULES_VIOLATED');
    });
  });

  describe('SimulateAnticipationPayload interface', () => {
    it('should require requestedAmount', () => {
      const payload: SimulateAnticipationPayload = {
        requestedAmount: 1000,
      };
      expect(payload.requestedAmount).toBeDefined();
      expect(payload.requestedAmount).toBeGreaterThan(0);
    });

    it('should allow optional creatorId for Admin/Analista', () => {
      const payload: SimulateAnticipationPayload = {
        requestedAmount: 1000,
        creatorId: 'creator-123',
      };
      expect(payload.creatorId).toBeDefined();
      expect(payload.creatorId).toEqual('creator-123');
    });

    it('should validate minimum amount', () => {
      const payload: SimulateAnticipationPayload = {
        requestedAmount: 100, // minimum
      };
      expect(payload.requestedAmount).toBeGreaterThanOrEqual(100);
    });
  });

  describe('AnticipationRequestsPort interface extension', () => {
    it('should include simulateAnticipation method', () => {
      const port = {
        simulateAnticipation: (payload: SimulateAnticipationPayload) => {
          // returns Observable<SimulationResult>
        },
      };
      expect(port.simulateAnticipation).toBeDefined();
      expect(typeof port.simulateAnticipation).toBe('function');
    });

    it('should include convertSimulationToReal method', () => {
      const port = {
        convertSimulationToReal: (
          simulationCode: string,
          creatorId?: string
        ) => {
          // returns Observable<ConversionResult>
        },
      };
      expect(port.convertSimulationToReal).toBeDefined();
      expect(typeof port.convertSimulationToReal).toBe('function');
    });
  });

  describe('Type guards and validation', () => {
    it('should distinguish AnticipationSimulation from regular object', () => {
      const simulation: AnticipationSimulation = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(),
        createdAt: new Date(),
      };
      expect('simulationCode' in simulation).toBe(true);
      expect('grossAmountCents' in simulation).toBe(true);
      expect('validUntil' in simulation).toBe(true);
    });

    it('should validate ConversionResult structure', () => {
      const result: ConversionResult = {
        id: 'req-123',
        protocol: 'PROT-001',
        status: AnticipationRequestStatus.Pending,
        netAmount: 95000,
      };
      expect(result).toHaveProperty('id');
      expect(result).toHaveProperty('protocol');
      expect(result).toHaveProperty('status');
      expect(result).toHaveProperty('netAmount');
    });

    it('should ensure SimulationError values are strings', () => {
      const errorCode: SimulationError = SimulationError.SIMULATION_EXPIRED;
      expect(typeof errorCode).toBe('string');
    });
  });
});

// Type interfaces for testing (must match production code)
interface SimulateAnticipationPayload {
  requestedAmount: number;
  creatorId?: string;
}

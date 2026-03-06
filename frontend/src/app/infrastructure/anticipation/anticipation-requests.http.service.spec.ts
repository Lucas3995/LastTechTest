// RF-1 + Plano 404 — Contrato da API de antecipação (listagem e cancelamento).
// Especificação executável: URLs, query params e formato de resposta alinhados ao backend.
// Fonte: relatório_404_minhas_solicitações_c5d4fe06.plan.md e árvore frontend-rf1-minhas-solicitacoes-arvore-testes.md §6.
//
// Nota: Estes testes definem o comportamento esperado após a implementação do plano. Até lá,
// 3 testes falham por URL/params (GET base sem /minhas, fromUtc/toUtc/page/pageSize, POST .../cancel)
// e 2 por mapeamento de resposta (items/totalCount → domínio, status backend → AnticipationRequestStatus).
//
// Plano 401: Este spec usa HttpClientTestingModule sem o auth-token interceptor; os testes permanecem
// independentes do interceptor (não verificam header Authorization). Ver árvore §5.3.

import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { firstValueFrom, Observable } from 'rxjs';

import {
  AnticipationAdminListFilter,
  AnticipationRequest,
  AnticipationRequestStatus,
  AnticipationRequestsFilter,
  CreateAnticipationRequestPayload,
} from '../../domain';
import { AnticipationRequestsHttpService } from './anticipation-requests.http.service';
import { API_BASE_URL } from '../../core/api-base-url';

const BASE_URL = 'https://api.example.com';

describe('AnticipationRequestsHttpService — Contrato API (plano 404)', () => {
  let service: AnticipationRequestsHttpService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AnticipationRequestsHttpService,
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    service = TestBed.inject(AnticipationRequestsHttpService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    // Flush any pending requests so verify() and next test can run (when a test fails by wrong URL, request stays open).
    const pending = httpMock.match(() => true);
    pending.forEach((req) => {
      if (req.request.method === 'GET') req.flush({ items: [], totalCount: 0 });
      else if (req.request.method === 'POST') req.flush({});
    });
    httpMock.verify();
    TestBed.resetTestingModule();
  });

  describe('PLAN-404-LIST: Listagem alinhada ao backend', () => {
    it('should call GET anticipations base URL for list (plan_404)', () => {
      service.listMyRequests().subscribe();

      const req = httpMock.expectOne(
        (r) =>
          r.url.startsWith(`${BASE_URL}/api/v1/anticipations`) &&
          !r.url.includes('/minhas') &&
          r.method === 'GET'
      );
      expect(req.request.method).toBe('GET');
      expect(req.request.url).toBe(`${BASE_URL}/api/v1/anticipations`);
      req.flush({ items: [], totalCount: 0 });
    });

    it('should send fromUtc toUtc status page pageSize in list (plan_404)', () => {
      const from = new Date('2025-01-01T00:00:00.000Z');
      const to = new Date('2025-01-31T23:59:59.999Z');
      const filter: AnticipationRequestsFilter = {
        period: { from, to },
        statuses: [AnticipationRequestStatus.Pending],
      };

      service.listMyRequests(filter).subscribe();

      const req = httpMock.expectOne(
        (r) =>
          r.url.startsWith(`${BASE_URL}/api/v1/anticipations`) &&
          !r.url.includes('/minhas') &&
          r.method === 'GET'
      );
      const params = req.request.params;
      // Backend espera fromUtc, toUtc, status (int?), page, pageSize
      expect(params.has('fromUtc')).toBe(true);
      expect(params.has('toUtc')).toBe(true);
      expect(params.has('status')).toBe(true);
      expect(params.has('page')).toBe(true);
      expect(params.has('pageSize')).toBe(true);
      req.flush({ items: [], totalCount: 0 });
    });

    it('should map response items and totalCount to domain list (plan_404)', async () => {
      const backendItem = {
        id: 'a1b2c3d4-0000-0000-0000-000000000001',
        protocol: 'ANT-001',
        creatorId: 'c1b2c3d4-0000-0000-0000-000000000002',
        status: 'Pending',
        requestedAmount: 100.5,
        netAmount: 95.0,
        requestedAtUtc: '2025-02-01T12:00:00Z',
        createdAtUtc: '2025-02-01T10:00:00Z',
      };

      const itemsPromise = firstValueFrom(service.listMyRequests());

      const req = httpMock.expectOne(
        (r) =>
          r.url.startsWith(`${BASE_URL}/api/v1/anticipations`) &&
          r.method === 'GET'
      );
      req.flush({
        items: [backendItem],
        totalCount: 1,
      });

      const items = await itemsPromise;
      expect(Array.isArray(items)).toBe(true);
      expect(items.length).toBe(1);
      expect(items[0].id).toBe(backendItem.id);
      expect(items[0].creatorId).toBe(backendItem.creatorId);
      expect(items[0].status).toBe(AnticipationRequestStatus.Pending);
      expect(items[0].grossAmountCents).toBeDefined();
      expect(items[0].netAmountCents).toBeDefined();
      expect(items[0].createdAt).toBeDefined();
    });
  });

  describe('PLAN-404-CANCEL: Cancelamento alinhado ao backend', () => {
    it('should call POST anticipations id cancel for cancel (plan_404)', () => {
      const id = 'a1b2c3d4-0000-0000-0000-000000000001';
      service.cancelRequest(id).subscribe();

      const req = httpMock.expectOne(
        (r) =>
          r.url === `${BASE_URL}/api/v1/anticipations/${id}/cancel` &&
          r.method === 'POST'
      );
      expect(req.request.method).toBe('POST');
      expect(req.request.url).not.toContain('cancelamento');
      req.flush({
        id,
        protocol: 'ANT-001',
        status: 'CanceledByCreator',
        alreadyCanceled: false,
      });
    });
  });

  describe('PLAN-404-STATUS: Mapeamento de status', () => {
    it('should map filter status to backend enum values (plan_404)', () => {
      const filter: AnticipationRequestsFilter = {
        statuses: [
          AnticipationRequestStatus.Pending,
          AnticipationRequestStatus.Approved,
        ],
      };

      service.listMyRequests(filter).subscribe();

      const req = httpMock.expectOne(
        (r) =>
          r.url.startsWith(`${BASE_URL}/api/v1/anticipations`) &&
          r.method === 'GET'
      );
      const statusParam = req.request.params.get('status');
      expect(statusParam).toBeTruthy();
      // Backend usa int (Pending=1, Approved=2); front pode enviar int ou string conforme contrato
      req.flush({ items: [], totalCount: 0 });
    });

    it('should map backend status to domain status (plan_404)', async () => {
      const backendItem = {
        id: 'e5f6-0000-0000-0000-000000000003',
        protocol: 'ANT-002',
        creatorId: 'c2-0000-0000-0000-000000000004',
        status: 'CanceledByCreator',
        requestedAmount: 200,
        netAmount: 190,
        requestedAtUtc: '2025-02-02T12:00:00Z',
        createdAtUtc: '2025-02-02T10:00:00Z',
      };

      const itemsPromise = firstValueFrom(service.listMyRequests());

      const req = httpMock.expectOne(
        (r) =>
          r.url.startsWith(`${BASE_URL}/api/v1/anticipations`) &&
          r.method === 'GET'
      );
      req.flush({ items: [backendItem], totalCount: 1 });

      const items = await itemsPromise;
      expect(items[0].status).toBe(AnticipationRequestStatus.CanceledByCreator);
    });
  });

  // RF-2 Lista global Admin — T2: listGlobalRequests (params + totalCount)
  describe('RF-2 listGlobalRequests (T2)', () => {
    it('should call GET anticipations with creatorId, status, fromUtc, toUtc, page, pageSize', () => {
      const filter: AnticipationAdminListFilter = {
        creatorId: 'creator-123',
        statuses: [AnticipationRequestStatus.Pending],
        period: {
          from: new Date('2025-01-01T00:00:00.000Z'),
          to: new Date('2025-01-31T23:59:59.999Z'),
        },
        page: 2,
        pageSize: 20,
      };

      service.listGlobalRequests(filter).subscribe();

      const req = httpMock.expectOne(
        (r) =>
          r.url === `${BASE_URL}/api/v1/anticipations` &&
          r.method === 'GET'
      );
      const params = req.request.params;
      expect(params.get('creatorId')).toBe('creator-123');
      expect(params.get('status')).toBeTruthy();
      expect(params.get('fromUtc')).toBe('2025-01-01T00:00:00.000Z');
      expect(params.get('toUtc')).toBe('2025-01-31T23:59:59.999Z');
      expect(params.get('page')).toBe('2');
      expect(params.get('pageSize')).toBe('20');
      req.flush({ items: [], totalCount: 0 });
    });

    it('should map response items and totalCount to domain (T2)', async () => {
      const backendItem = {
        id: 'req-global-1',
        protocol: 'ANT-G-001',
        creatorId: 'creator-global',
        status: 'Pending',
        requestedAmount: 50.0,
        netAmount: 47.5,
        requestedAtUtc: '2025-02-10T12:00:00Z',
        createdAtUtc: '2025-02-10T10:00:00Z',
      };

      const resultPromise = firstValueFrom(
        service.listGlobalRequests({ page: 1, pageSize: 10 }),
      );

      const req = httpMock.expectOne(
        (r) =>
          r.url.startsWith(`${BASE_URL}/api/v1/anticipations`) &&
          r.method === 'GET'
      );
      req.flush({
        items: [backendItem],
        totalCount: 1,
      });

      const result = await resultPromise;
      expect(result.items).toBeDefined();
      expect(result.totalCount).toBe(1);
      expect(result.items.length).toBe(1);
      expect(result.items[0].id).toBe(backendItem.id);
      expect(result.items[0].creatorId).toBe(backendItem.creatorId);
      expect(result.items[0].status).toBe(AnticipationRequestStatus.Pending);
    });
  });

  // RF-4 Aprovar/Recusar — POST approve e reject, body e mapeamento da resposta (id, protocol, status) para domínio.
  // Serviço implementará approveRequest e rejectRequest; até lá os testes falham por método inexistente.
  describe('RF-4 approveRequest', () => {
    it('should send POST to .../id/approve with body observation and map response id protocol status to domain', async () => {
      const id = 'req-approve-1';
      const observation = 'Aprovado conforme política.';
      const backendResponse = { id, protocol: 'ANT-001', status: 'Approved' };

      const svc = service as AnticipationRequestsHttpService & {
        approveRequest(id: string, observation?: string): Observable<AnticipationRequest>;
      };
      const resultPromise = firstValueFrom(svc.approveRequest(id, observation));

      const req = httpMock.expectOne(
        (r) =>
          r.url === `${BASE_URL}/api/v1/anticipations/${id}/approve` &&
          r.method === 'POST',
      );
      expect(req.request.body).toEqual({ observation });
      req.flush(backendResponse);

      const result = await resultPromise;
      expect(result.id).toBe(id);
      expect(result.status).toBe(AnticipationRequestStatus.Approved);
    });
  });

  describe('RF-4 rejectRequest', () => {
    it('should send POST to .../id/reject with body reason and map response id protocol status to domain', async () => {
      const id = 'req-reject-1';
      const reason = 'Documentação incompleta.';
      const backendResponse = { id, protocol: 'ANT-002', status: 'Rejected' };

      const svc = service as AnticipationRequestsHttpService & {
        rejectRequest(id: string, reason: string): Observable<AnticipationRequest>;
      };
      const resultPromise = firstValueFrom(svc.rejectRequest(id, reason));

      const req = httpMock.expectOne(
        (r) =>
          r.url === `${BASE_URL}/api/v1/anticipations/${id}/reject` &&
          r.method === 'POST',
      );
      expect(req.request.body).toEqual({ reason });
      req.flush(backendResponse);

      const result = await resultPromise;
      expect(result.id).toBe(id);
      expect(result.status).toBe(AnticipationRequestStatus.Rejected);
    });
  });

  // RF-5 Criar solicitação — POST createRequest, body e mapeamento 201 → CreateAnticipationRequestResult
  describe('RF-5 createRequest', () => {
    it('createRequest sends POST to baseUrl with body requestedAmount (and optional creatorId)', () => {
      const payload: CreateAnticipationRequestPayload = { requestedAmount: 500 };
      service.createRequest(payload).subscribe();

      const req = httpMock.expectOne(
        (r) =>
          r.url === `${BASE_URL}/api/v1/anticipations` &&
          r.method === 'POST',
      );
      expect(req.request.body).toEqual({ requestedAmount: 500 });
      req.flush({
        id: 'new-req-1',
        protocol: 'ANT-NEW-001',
        netAmount: 475,
        status: 'Pending',
      });
    });

    it('createRequest sends creatorId when provided', () => {
      const payload: CreateAnticipationRequestPayload = {
        requestedAmount: 1000,
        creatorId: 'creator-admin-123',
      };
      service.createRequest(payload).subscribe();

      const req = httpMock.expectOne(
        (r) =>
          r.url === `${BASE_URL}/api/v1/anticipations` &&
          r.method === 'POST',
      );
      expect(req.request.body).toEqual({
        requestedAmount: 1000,
        creatorId: 'creator-admin-123',
      });
      req.flush({
        id: 'new-req-2',
        protocol: 'ANT-NEW-002',
        netAmount: 950,
        status: 'Pending',
      });
    });

    it('createRequest maps 201 response Id Protocol NetAmount Status to CreateAnticipationRequestResult', async () => {
      const payload: CreateAnticipationRequestPayload = { requestedAmount: 300 };
      const backendResponse = {
        id: 'created-id-1',
        protocol: 'ANT-100',
        netAmount: 285,
        status: 'Pending',
      };

      const resultPromise = firstValueFrom(service.createRequest(payload));

      const req = httpMock.expectOne(
        (r) =>
          r.url === `${BASE_URL}/api/v1/anticipations` &&
          r.method === 'POST',
      );
      req.flush(backendResponse);

      const result = await resultPromise;
      expect(result.id).toBe(backendResponse.id);
      expect(result.protocol).toBe(backendResponse.protocol);
      expect(result.netAmount).toBe(backendResponse.netAmount);
      expect(result.status).toBe(AnticipationRequestStatus.Pending);
    });
  });

  describe('RF-3-SIMULATION: Simulação de Antecipação (CA-RF3-1 a CA-RF3-6)', () => {
    describe('simulateAnticipation()', () => {
      it('[CA-RF3-1] should POST to /simulations endpoint', () => {
        const payload = { requestedAmount: 1000 };
        service.simulateAnticipation(payload).subscribe();

        const req = httpMock.expectOne(
          (r) =>
            r.url === `${BASE_URL}/api/v1/anticipations/simulations` &&
            r.method === 'POST'
        );
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(payload);
        req.flush({
          simulationCode: 'SIM-123456',
          grossAmount: 1000,
          feesAmount: 50,
          netAmount: 950,
          validUntilUtc: new Date().toISOString(),
          requestedAmount: 1000
        });
      });

      it('[CA-RF3-1] should map response to SimulationResult', async () => {
        const payload = { requestedAmount: 1000 };
        const backendResponse = {
          simulationCode: 'SIM-ABC123',
          grossAmount: 1000,
          feesAmount: 50,
          netAmount: 950,
          validUntilUtc: '2026-03-06T11:40:00Z',
          requestedAmount: 1000
        };

        const resultPromise = firstValueFrom(
          service.simulateAnticipation(payload)
        );

        const req = httpMock.expectOne(
          (r) =>
            r.url === `${BASE_URL}/api/v1/anticipations/simulations` &&
            r.method === 'POST'
        );
        req.flush(backendResponse);

        const result = await resultPromise;
        expect(result.simulationCode).toBe(backendResponse.simulationCode);
        expect(result.grossAmountCents).toBe(100000);
        expect(result.feesAmountCents).toBe(5000);
        expect(result.netAmountCents).toBe(95000);
        expect(result.validUntil).toBeDefined();
        expect(result.validUntil instanceof Date).toBe(true);
      });

      it('[CA-RF3-2] should handle validation errors (400)', async () => {
        const payload = { requestedAmount: 50 }; // below minimum (100)
        const errorPromise = firstValueFrom(
          service.simulateAnticipation(payload)
        ).catch((error) => error);

        const req = httpMock.expectOne(
          (r) =>
            r.url === `${BASE_URL}/api/v1/anticipations/simulations` &&
            r.method === 'POST'
        );
        req.flush(
          {
            message: 'Validation failed',
            errors: [{ field: 'requestedAmount', message: 'Mínimo R$ 100,00' }],
          },
          { status: 400, statusText: 'Bad Request' }
        );

        const error = await errorPromise;
        expect(error).toBeDefined();
        expect(error.status).toBe(400);
      });

      it('[CA-RF3-2] should handle server errors (500)', async () => {
        const payload = { requestedAmount: 1000 };
        const errorPromise = firstValueFrom(
          service.simulateAnticipation(payload)
        ).catch((error) => error);

        const req = httpMock.expectOne(
          (r) =>
            r.url === `${BASE_URL}/api/v1/anticipations/simulations` &&
            r.method === 'POST'
        );
        req.flush(
          { error: 'Internal server error' },
          { status: 500, statusText: 'Internal Server Error' }
        );

        const error = await errorPromise;
        expect(error).toBeDefined();
        expect(error.status).toBe(500);
      });

      it('[CA-RF3-6] should accept creatorId for Admin/Analista simulation', () => {
        const payload = {
          requestedAmount: 1000,
          creatorId: 'creator-user-123',
        };
        service.simulateAnticipation(payload).subscribe();

        const req = httpMock.expectOne(
          (r) =>
            r.url === `${BASE_URL}/api/v1/anticipations/simulations` &&
            r.method === 'POST'
        );
        expect(req.request.body).toEqual(payload);
        expect(req.request.body.creatorId).toBe('creator-user-123');
        req.flush({
          simulationCode: 'SIM-ADMIN-001',
          grossAmountCents: 100000,
          feesAmountCents: 5000,
          netAmountCents: 95000,
          validUntil: new Date().toISOString(),
        });
      });
    });

    describe('convertSimulationToReal()', () => {
      const simulationCode = 'SIM-123456';

      it('[CA-RF3-4] should POST to /simulations/{code}/confirm endpoint', () => {
        service.convertSimulationToReal(simulationCode).subscribe();

        const req = httpMock.expectOne(
          (r) =>
            r.url ===
              `${BASE_URL}/api/v1/anticipations/simulations/${simulationCode}/confirm` &&
            r.method === 'POST'
        );
        expect(req.request.method).toBe('POST');
        expect(
          req.request.url.includes(`/simulations/${simulationCode}/confirm`)
        ).toBe(true);
        req.flush({
          id: 'req-789',
          protocol: 'PROT-001',
          status: 'Pending',
          netAmount: 950,
        });
      });

      it('[CA-RF3-4] should handle conversion success (201/200)', async () => {
        const backendResponse = {
          id: 'req-123456',
          protocol: 'PROT-20260306-001',
          status: 'Pending',
          netAmount: 95000,
        };

        const resultPromise = firstValueFrom(
          service.convertSimulationToReal(simulationCode)
        );

        const req = httpMock.expectOne(
          (r) =>
            r.url ===
              `${BASE_URL}/api/v1/anticipations/simulations/${simulationCode}/confirm` &&
            r.method === 'POST'
        );
        req.flush(backendResponse);

        const result = await resultPromise;
        expect(result.id).toBe(backendResponse.id);
        expect(result.protocol).toBe(backendResponse.protocol);
        expect(result.status).toBe(AnticipationRequestStatus.Pending);
        expect(result.netAmount).toBe(backendResponse.netAmount);
      });

      it('[CA-RF3-5] should handle expired simulation (422)', async () => {
        const errorPromise = firstValueFrom(
          service.convertSimulationToReal(simulationCode)
        ).catch((error) => error);

        const req = httpMock.expectOne(
          (r) =>
            r.url ===
              `${BASE_URL}/api/v1/anticipations/simulations/${simulationCode}/confirm` &&
            r.method === 'POST'
        );
        req.flush(
          {
            code: 'SIMULATION_EXPIRED',
            message: 'Simulação expirou. Realize uma nova simulação.',
          },
          { status: 422, statusText: 'Unprocessable Entity' }
        );

        const error = await errorPromise;
        expect(error.status).toBe(422);
      });

      it('[CA-RF3-5] should handle pending request exists (422)', async () => {
        const errorPromise = firstValueFrom(
          service.convertSimulationToReal(simulationCode)
        ).catch((error) => error);

        const req = httpMock.expectOne(
          (r) =>
            r.url ===
              `${BASE_URL}/api/v1/anticipations/simulations/${simulationCode}/confirm` &&
            r.method === 'POST'
        );
        req.flush(
          {
            code: 'PENDING_EXISTS',
            message:
              'Você já possui uma solicitação em aberto. Conclua ou cancele-a antes.',
          },
          { status: 422, statusText: 'Unprocessable Entity' }
        );

        const error = await errorPromise;
        expect(error.status).toBe(422);
      });

      it('[CA-RF3-5] should handle rules violation (422)', async () => {
        const errorPromise = firstValueFrom(
          service.convertSimulationToReal(simulationCode)
        ).catch((error) => error);

        const req = httpMock.expectOne(
          (r) =>
            r.url ===
              `${BASE_URL}/api/v1/anticipations/simulations/${simulationCode}/confirm` &&
            r.method === 'POST'
        );
        req.flush(
          {
            code: 'RULES_VIOLATED',
            message:
              'As condições de negócio mudaram. Realize uma nova simulação.',
          },
          { status: 422, statusText: 'Unprocessable Entity' }
        );

        const error = await errorPromise;
        expect(error.status).toBe(422);
      });

      it('[CA-RF3-5] should handle already used simulation (422)', async () => {
        const errorPromise = firstValueFrom(
          service.convertSimulationToReal(simulationCode)
        ).catch((error) => error);

        const req = httpMock.expectOne(
          (r) =>
            r.url ===
              `${BASE_URL}/api/v1/anticipations/simulations/${simulationCode}/confirm` &&
            r.method === 'POST'
        );
        req.flush(
          {
            code: 'ALREADY_USED',
            message: 'Esta simulação já foi utilizada para criar uma solicitação.',
          },
          { status: 422, statusText: 'Unprocessable Entity' }
        );

        const error = await errorPromise;
        expect(error.status).toBe(422);
      });

      it('[CA-RF3-6] should include creatorId in payload for Admin/Analista', () => {
        const creatorId = 'creator-user-123';
        service.convertSimulationToReal(simulationCode, creatorId).subscribe();

        const req = httpMock.expectOne(
          (r) =>
            r.url ===
              `${BASE_URL}/api/v1/anticipations/simulations/${simulationCode}/confirm` &&
            r.method === 'POST'
        );
        expect(req.request.body).toEqual({ creatorId });
        req.flush({
          id: 'req-admin-001',
          protocol: 'PROT-ADM-001',
          status: 'Pending',
          netAmount: 95000,
        });
      });
    });
  });
});

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
import { firstValueFrom } from 'rxjs';

import {
  AnticipationRequestStatus,
  AnticipationRequestsFilter,
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
});

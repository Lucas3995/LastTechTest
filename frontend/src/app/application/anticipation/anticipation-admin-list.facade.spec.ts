// RF-2 Lista global Admin — T1: AnticipationAdminListFacade (unit + integration)
// Plano árvore testes RF-2: loadInitial, applyFilters, clearFilters, setPage, selectRequest, clearMessages, error/supportId, totalCount.

import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import {
  AnticipationRequest,
  AnticipationRequestStatus,
  AnticipationAdminListFilter,
  ANTICIPATION_REQUESTS_PORT,
  ERROR_PRESENTATION_BUILDER,
} from '../../domain';
import { AnticipationAdminListFacade } from './anticipation-admin-list.facade';

function createRequest(overrides: Partial<AnticipationRequest> = {}): AnticipationRequest {
  return {
    id: 'req-1',
    creatorId: 'creator-1',
    createdAt: '2025-01-15T10:00:00Z',
    grossAmountCents: 10000,
    netAmountCents: 9500,
    status: AnticipationRequestStatus.Pending,
    ...overrides,
  };
}

describe('AnticipationAdminListFacade — RF-2 (T1)', () => {
  let facade: AnticipationAdminListFacade;
  let listGlobalRequestsSpy: ReturnType<typeof vi.fn>;
  let getRequestDetailSpy: ReturnType<typeof vi.fn>;
  let approveRequestSpy: ReturnType<typeof vi.fn>;
  let rejectRequestSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    listGlobalRequestsSpy = vi.fn().mockReturnValue(of({ items: [], totalCount: 0 }));
    getRequestDetailSpy = vi.fn().mockReturnValue(of(createRequest()));
    approveRequestSpy = vi.fn().mockReturnValue(of(createRequest({ id: 'req-1', status: AnticipationRequestStatus.Approved })));
    rejectRequestSpy = vi.fn().mockReturnValue(of(createRequest({ id: 'req-1', status: AnticipationRequestStatus.Rejected })));

    TestBed.configureTestingModule({
      providers: [
        AnticipationAdminListFacade,
        {
          provide: ANTICIPATION_REQUESTS_PORT,
          useValue: {
            listGlobalRequests: listGlobalRequestsSpy,
            getRequestDetail: getRequestDetailSpy,
            listMyRequests: vi.fn(),
            cancelRequest: vi.fn(),
            approveRequest: approveRequestSpy,
            rejectRequest: rejectRequestSpy,
          },
        },
        {
          provide: ERROR_PRESENTATION_BUILDER,
          useValue: (error: unknown, contextMessage: string) => ({
            contextMessage,
            supportId: (error as { supportId?: string })?.supportId,
          }),
        },
      ],
    });
    facade = TestBed.inject(AnticipationAdminListFacade);
  });

  describe('loadInitial', () => {
    it('should load initial list with default period', async () => {
      listGlobalRequestsSpy.mockReturnValue(
        of({ items: [createRequest({ id: 'r1' })], totalCount: 1 }),
      );

      await facade.loadInitial();

      expect(listGlobalRequestsSpy).toHaveBeenCalled();
      const filter = listGlobalRequestsSpy.mock.calls[0][0] as AnticipationAdminListFilter;
      expect(filter.page).toBe(1);
      expect(filter.pageSize).toBe(20);
      expect(filter.period).toBeDefined();
      expect(facade.requests().length).toBe(1);
      expect(facade.totalCount()).toBe(1);
    });
  });

  describe('applyFilters', () => {
    it('should apply filters and call port with creatorId, statuses, period, page, pageSize', async () => {
      const filter: AnticipationAdminListFilter = {
        creatorId: 'creator-123',
        statuses: [AnticipationRequestStatus.Pending],
        period: {
          from: new Date('2025-01-01T00:00:00Z'),
          to: new Date('2025-01-31T23:59:59Z'),
        },
        page: 1,
        pageSize: 25,
      };

      await facade.applyFilters(filter);

      expect(listGlobalRequestsSpy).toHaveBeenCalledWith(filter);
      expect(listGlobalRequestsSpy).toHaveBeenCalledTimes(1);
    });
  });

  describe('clearFilters', () => {
    it('should clear filters and reload', async () => {
      listGlobalRequestsSpy.mockReturnValue(of({ items: [], totalCount: 0 }));

      await facade.applyFilters({ page: 1, pageSize: 10, creatorId: 'c1' });
      expect(listGlobalRequestsSpy).toHaveBeenCalledWith(
        expect.objectContaining({ creatorId: 'c1' }),
      );

      listGlobalRequestsSpy.mockClear();
      await facade.clearFilters();

      expect(listGlobalRequestsSpy).toHaveBeenCalled();
    });
  });

  describe('setPage', () => {
    it('should set page and reload', async () => {
      const filter: AnticipationAdminListFilter = {
        page: 1,
        pageSize: 20,
      };
      await facade.applyFilters(filter);

      listGlobalRequestsSpy.mockClear();
      await facade.setPage(2);

      expect(listGlobalRequestsSpy).toHaveBeenCalledWith(
        expect.objectContaining({ page: 2, pageSize: 20 }),
      );
    });
  });

  describe('selectRequest', () => {
    it('should select request and load detail', async () => {
      const req = createRequest({ id: 'detail-1' });
      getRequestDetailSpy.mockReturnValue(of(req));

      await facade.selectRequest('detail-1');

      expect(getRequestDetailSpy).toHaveBeenCalledWith('detail-1');
      expect(facade.selectedRequest()).toEqual(req);
    });

    it('should expose error with supportId on port error', async () => {
      getRequestDetailSpy.mockReturnValue(
        throwError(() => ({ message: 'Not found', supportId: 'SUP-123' })),
      );

      await facade.selectRequest('missing-id');

      expect(facade.errorMessage()).toBeTruthy();
      expect(facade.errorSupportId()).toBe('SUP-123');
    });
  });

  describe('clearMessages', () => {
    it('should clear error and supportId', async () => {
      getRequestDetailSpy.mockReturnValue(
        throwError(() => ({ message: 'Err', supportId: 'S1' })),
      );
      await facade.selectRequest('x');
      expect(facade.errorMessage()).toBeTruthy();

      facade.clearMessages();

      expect(facade.errorMessage()).toBeNull();
      expect(facade.errorSupportId()).toBeNull();
    });
  });

  describe('totalCount', () => {
    it('should expose totalCount from listGlobalRequests', async () => {
      listGlobalRequestsSpy.mockReturnValue(
        of({ items: [createRequest(), createRequest()], totalCount: 42 }),
      );

      await facade.loadInitial();

      expect(facade.totalCount()).toBe(42);
      expect(facade.requests().length).toBe(2);
    });
  });

  // RF-4 Aprovar/Recusar — approveRequest e rejectRequest (sucesso e erro).
  describe('approveRequest', () => {
    it('should call port approveRequest with id and optional observation and on success update selectedRequest and set infoMessage', async () => {
      const approved = createRequest({ id: 'req-ap', status: AnticipationRequestStatus.Approved });
      getRequestDetailSpy.mockReturnValue(of(createRequest({ id: 'req-ap' })));
      approveRequestSpy.mockReturnValue(of(approved));
      await facade.selectRequest('req-ap');
      expect(facade.selectedRequest()?.id).toBe('req-ap');

      const facadeWithApprove = facade as AnticipationAdminListFacade & {
        approveRequest(id: string, observation?: string): Promise<void>;
      };
      await facadeWithApprove.approveRequest('req-ap', 'Ok aprovado.');

      expect(approveRequestSpy).toHaveBeenCalledWith('req-ap', 'Ok aprovado.');
      expect(facade.selectedRequest()).toEqual(approved);
      expect(facade.infoMessage()).toBeTruthy();
      expect(facade.errorMessage()).toBeNull();
    });

    it('should set errorMessage and errorSupportId on port error and not change selectedRequest', async () => {
      const current = createRequest({ id: 'req-err' });
      getRequestDetailSpy.mockReturnValue(of(current));
      await facade.selectRequest('req-err');
      approveRequestSpy.mockReturnValue(
        throwError(() => ({ message: 'Forbidden', supportId: 'SUP-456' })),
      );

      const facadeWithApprove = facade as AnticipationAdminListFacade & {
        approveRequest(id: string, observation?: string): Promise<void>;
      };
      await facadeWithApprove.approveRequest('req-err');

      expect(facade.errorMessage()).toBeTruthy();
      expect(facade.errorSupportId()).toBe('SUP-456');
      expect(facade.selectedRequest()).toEqual(current);
    });
  });

  describe('rejectRequest', () => {
    it('should call port rejectRequest with id and reason and on success update selectedRequest and set infoMessage', async () => {
      const rejected = createRequest({ id: 'req-rj', status: AnticipationRequestStatus.Rejected });
      getRequestDetailSpy.mockReturnValue(of(createRequest({ id: 'req-rj' })));
      rejectRequestSpy.mockReturnValue(of(rejected));
      await facade.selectRequest('req-rj');

      const facadeWithReject = facade as AnticipationAdminListFacade & {
        rejectRequest(id: string, reason: string): Promise<void>;
      };
      await facadeWithReject.rejectRequest('req-rj', 'Motivo da recusa.');

      expect(rejectRequestSpy).toHaveBeenCalledWith('req-rj', 'Motivo da recusa.');
      expect(facade.selectedRequest()).toEqual(rejected);
      expect(facade.infoMessage()).toBeTruthy();
      expect(facade.errorMessage()).toBeNull();
    });

    it('should set errorMessage and errorSupportId on port error and not change selectedRequest', async () => {
      const current = createRequest({ id: 'req-err2' });
      getRequestDetailSpy.mockReturnValue(of(current));
      await facade.selectRequest('req-err2');
      rejectRequestSpy.mockReturnValue(
        throwError(() => ({ message: 'Bad request', supportId: 'SUP-789' })),
      );

      const facadeWithReject = facade as AnticipationAdminListFacade & {
        rejectRequest(id: string, reason: string): Promise<void>;
      };
      await facadeWithReject.rejectRequest('req-err2', 'reason');

      expect(facade.errorMessage()).toBeTruthy();
      expect(facade.errorSupportId()).toBe('SUP-789');
      expect(facade.selectedRequest()).toEqual(current);
    });
  });
});

// RF-5 Criar solicitação — AnticipationMyRequestsFacade createRequest (T-RF5-1)
// Plano árvore testes RF-5: createRequest chama port com payload; sucesso → loadWithFilter + infoMessage; erro → setErrorFromPresentation.

import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import {
  AnticipationRequest,
  AnticipationRequestStatus,
  AnticipationRequestsFilter,
  ANTICIPATION_REQUESTS_PORT,
  CreateAnticipationRequestPayload,
  ERROR_PRESENTATION_BUILDER,
} from '../../domain';
import { AnticipationMyRequestsFacade } from './anticipation-my-requests.facade';

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

describe('AnticipationMyRequestsFacade — RF-5 createRequest (T-RF5-1)', () => {
  let facade: AnticipationMyRequestsFacade;
  let createRequestPortSpy: ReturnType<typeof vi.fn>;
  let listMyRequestsSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    createRequestPortSpy = vi.fn().mockReturnValue(
      of({ id: 'new-1', protocol: 'ANT-001', netAmount: 475, status: AnticipationRequestStatus.Pending }),
    );
    listMyRequestsSpy = vi.fn().mockReturnValue(of([]));

    TestBed.configureTestingModule({
      providers: [
        AnticipationMyRequestsFacade,
        {
          provide: ANTICIPATION_REQUESTS_PORT,
          useValue: {
            listMyRequests: listMyRequestsSpy,
            listGlobalRequests: vi.fn(),
            getRequestDetail: vi.fn(),
            cancelRequest: vi.fn(),
            approveRequest: vi.fn(),
            rejectRequest: vi.fn(),
            createRequest: createRequestPortSpy,
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
    facade = TestBed.inject(AnticipationMyRequestsFacade);
  });

  describe('createRequest', () => {
    it('createRequest calls port with correct payload (requestedAmount)', async () => {
      const payload: CreateAnticipationRequestPayload = { requestedAmount: 500 };

      await facade.createRequest(payload);

      expect(createRequestPortSpy).toHaveBeenCalledWith(payload);
      expect(createRequestPortSpy).toHaveBeenCalledTimes(1);
    });

    it('createRequest on success clears error, sets infoMessage to "Solicitação criada com sucesso." and calls loadWithFilter', async () => {
      facade.clearMessages();
      await facade.loadInitialRequests();
      listMyRequestsSpy.mockClear();
      createRequestPortSpy.mockReturnValue(
        of({ id: 'new-1', protocol: 'ANT-001', netAmount: 500, status: AnticipationRequestStatus.Pending }),
      );

      await facade.createRequest({ requestedAmount: 500 });

      expect(facade.infoMessage()).toBe('Solicitação criada com sucesso.');
      expect(facade.errorMessage()).toBeNull();
      expect(listMyRequestsSpy).toHaveBeenCalled();
    });

    it('createRequest on error calls setErrorFromPresentation with context message', async () => {
      const err = new Error('API error');
      createRequestPortSpy.mockReturnValue(throwError(() => err));

      await facade.createRequest({ requestedAmount: 1000 });

      expect(facade.errorMessage()).toBeTruthy();
      expect(facade.errorMessage()).toContain('Nao foi possivel criar a solicitacao de antecipacao');
    });
  });
});

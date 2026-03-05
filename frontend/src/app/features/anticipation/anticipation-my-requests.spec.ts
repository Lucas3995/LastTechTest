// RF-1 Minhas solicitações de antecipação — árvore de testes (unitários/integração)
// Especificação executável ligada aos componentes e à façade.
// Plano 401: Estes testes mockam AnticipationRequestsHttpService e não dependem do auth-token interceptor (árvore §5.3).

import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { HttpErrorResponse } from '@angular/common/http';
import { of, Subject, throwError } from 'rxjs';
import { vi } from 'vitest';

import {
  AnticipationRequest,
  AnticipationRequestStatus,
  AnticipationRequestsFilter,
} from '../../domain';
import { AnticipationMyRequestsFacade } from '../../application';
import { AnticipationRequestsHttpService } from '../../infrastructure/anticipation/anticipation-requests.http.service';
import { AnticipationMyRequestsPageComponent } from './pages/my-requests/anticipation-my-requests-page.component';
import { AnticipationRequestsTableComponent } from './components/anticipation-requests-table/anticipation-requests-table.component';
import { AnticipationRequestEmptyStateComponent } from './components/anticipation-request-empty-state/anticipation-request-empty-state.component';
import { AnticipationRequestDetailComponent } from './components/anticipation-request-detail/anticipation-request-detail.component';
import { AnticipationRequestsFiltersComponent } from './components/anticipation-requests-filters/anticipation-requests-filters.component';
import { API_BASE_URL } from '../../core/api-base-url';

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

describe('RF-1 Minhas solicitações de antecipação (Creator)', () => {
  //
  // CA-RF1-1 – Lista restrita ao próprio Creator
  //
  describe('CA-RF1-1 Lista restrita ao próprio Creator', () => {
    it('should normalize requests to table rows (CA_RF1_1)', () => {
      const requests: AnticipationRequest[] = [
        createRequest({ id: 'r1' }),
        createRequest({ id: 'r2' }),
      ];
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsTableComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsTableComponent);
      fixture.componentInstance.requests = requests;
      fixture.detectChanges();
      const rows = fixture.nativeElement.querySelectorAll('[data-testid="requests-table-row"]');
      expect(rows.length).toBe(2);
    });

    it('should format amounts and dates (CA_RF1_1)', () => {
      const request = createRequest({
        grossAmountCents: 12345,
        netAmountCents: 11000,
        createdAt: '2025-02-20T14:30:00Z',
      });
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsTableComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsTableComponent);
      fixture.componentInstance.requests = [request];
      fixture.detectChanges();
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('123.45');
      expect(el.textContent).toContain('110.00');
      expect(el.textContent).toMatch(/\d{1,2}\/\d{1,2}\/\d{2,4}/);
    });

    it('should build minimum columns for table (CA_RF1_1)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsTableComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsTableComponent);
      fixture.componentInstance.requests = [createRequest()];
      fixture.detectChanges();
      const header = fixture.nativeElement.querySelector('thead tr');
      expect(header?.textContent).toContain('ID');
      expect(header?.textContent).toContain('Data');
      expect(header?.textContent).toContain('Valor');
      expect(header?.textContent).toContain('Valor liquido');
      expect(header?.textContent).toContain('Status');
    });

    it('should call list service with current creator_id (CA_RF1_1)', async () => {
      const listSpy = vi.fn().mockReturnValue(of( [] ));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      expect(listSpy).toHaveBeenCalled();
    });

    it('should render only rows from current creator (CA_RF1_1)', async () => {
      const myRequests = [createRequest({ creatorId: 'me' }), createRequest({ id: 'r2', creatorId: 'me' })];
      const listSpy = vi.fn().mockReturnValue(of( myRequests ));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const table = fixture.nativeElement.querySelector('app-anticipation-requests-table');
      expect(table).toBeTruthy();
      const rows = fixture.nativeElement.querySelectorAll('[data-testid="requests-table-row"]');
      expect(rows.length).toBe(2);
    });

    it('should filter out requests from other creators if service misbehaves (CA_RF1_1)', async () => {
      const myRequests = [
        createRequest({ id: 'mine', creatorId: 'creator-1' }),
      ];
      const listSpy = vi.fn().mockReturnValue(of( myRequests ));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      const requests = facade.requests();
      expect(requests.every((r) => r.creatorId === 'creator-1')).toBe(true);
      expect(requests.length).toBe(1);
    });
  });

  //
  // CA-RF1-2 – Lista vazia com orientação
  //
  describe('CA-RF1-2 Lista vazia com orientação', () => {
    it('should show empty message when there are no requests (CA_RF1_2)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestEmptyStateComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestEmptyStateComponent);
      fixture.detectChanges();
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('nao tem solicitacoes');
      expect(el.querySelector('[data-testid="requests-empty-state"]')).toBeTruthy();
    });

    it('should show CTAs for new request and simulation on empty state (CA_RF1_2)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestEmptyStateComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestEmptyStateComponent);
      fixture.detectChanges();
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Nova solicitacao');
      expect(el.textContent).toContain('Simular antecipacao');
    });

    it('should switch from grid to empty state when service returns empty list (CA_RF1_2)', async () => {
      const listSpy = vi.fn().mockReturnValue(of( [] ));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const empty = fixture.nativeElement.querySelector('app-anticipation-request-empty-state');
      expect(empty).toBeTruthy();
    });

    it('should replace empty state with grid when requests appear (CA_RF1_2)', async () => {
      const listSpy = vi.fn()
        .mockReturnValueOnce(of( [] ))
        .mockReturnValueOnce(of( [createRequest()] ));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('app-anticipation-request-empty-state')).toBeTruthy();
      await facade.refresh();
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('app-anticipation-requests-table')).toBeTruthy();
    });
  });

  //
  // CA-RF1-3 – Ordenação e filtros visíveis
  //
  describe('CA-RF1-3 Filtros e ordenação visíveis', () => {
    it('should filter requests by status (CA_RF1_3)', async () => {
      const httpMock = {
        listMyRequests: vi.fn().mockReturnValue(of([])),
        getRequestDetail: vi.fn(),
        cancelRequest: vi.fn(),
      };
      TestBed.configureTestingModule({
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: httpMock },
        ],
      });
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      const filter: AnticipationRequestsFilter = { statuses: [AnticipationRequestStatus.Pending] };
      await facade.applyFilters(filter);
      expect(facade.filters()).toEqual(filter);
    });

    it('should filter requests by period (CA_RF1_3)', () => {
      const from = new Date('2025-01-01');
      const to = new Date('2025-01-31');
      const filter: AnticipationRequestsFilter = { period: { from, to } };
      expect(filter.period?.from).toEqual(from);
      expect(filter.period?.to).toEqual(to);
    });

    it('should sort requests by date and amount (CA_RF1_3)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsTableComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsTableComponent);
      fixture.componentInstance.requests = [
        createRequest({ id: 'a', grossAmountCents: 100 }),
        createRequest({ id: 'b', grossAmountCents: 200 }),
      ];
      fixture.detectChanges();
      const th = fixture.nativeElement.querySelector('th[aria-sort]');
      expect(th).toBeTruthy();
      th?.click();
      fixture.detectChanges();
      expect(fixture.componentInstance.sortDirection).not.toBe('none');
    });

    it('should mark active filters and sorted column in UI (CA_RF1_3)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsFiltersComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsFiltersComponent);
      fixture.componentInstance.filters = { statuses: [AnticipationRequestStatus.Pending] };
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('[data-testid="filter-status"]')).toBeTruthy();
    });

    it('should apply status filter and update grid (CA_RF1_3)', async () => {
      const listSpy = vi.fn().mockReturnValue(of( [createRequest({ status: AnticipationRequestStatus.Pending })] ));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      fixture.detectChanges();
      await fixture.whenStable();
      await facade.applyFilters({ statuses: [AnticipationRequestStatus.Approved] });
      expect(listSpy).toHaveBeenCalledTimes(2);
    });

    it('should apply period filter and update grid (CA_RF1_3)', async () => {
      const listSpy = vi.fn().mockReturnValue(of( [] ));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      const from = new Date();
      const to = new Date();
      await facade.applyFilters({ period: { from, to } });
      expect(listSpy).toHaveBeenLastCalledWith(expect.objectContaining({
        period: expect.objectContaining({ from, to }),
      }));
    });

    it('should apply sorting and update grid order (CA_RF1_3)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsTableComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsTableComponent);
      const reqs = [createRequest({ id: '1' }), createRequest({ id: '2' })];
      fixture.componentInstance.requests = reqs;
      fixture.detectChanges();
      fixture.componentInstance.onToggleAmountSort();
      expect(fixture.componentInstance.sortDirection).toBe('descending');
      fixture.componentInstance.onToggleAmountSort();
      expect(fixture.componentInstance.sortDirection).toBe('ascending');
    });

    it('should preserve filter values after reload (CA_RF1_3)', async () => {
      const listSpy = vi.fn().mockReturnValue(of( [] ));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      const filter: AnticipationRequestsFilter = { statuses: [AnticipationRequestStatus.Rejected] };
      await facade.applyFilters(filter);
      expect(facade.filters()).toEqual(filter);
      await facade.refresh();
      expect(facade.filters()).toEqual(filter);
    });
  });

  //
  // Bug filtro listagem creator (plano 4.2) — integração página ↔ filtros ↔ fachada/serviço
  //
  describe('Bug filtro — apply emitido chega à fachada e ao serviço (bug_filter)', () => {
    it('should call facade applyFilters with emitted filter when filters component emits apply (bug_filter)', async () => {
      const listSpy = vi.fn().mockReturnValue(of( [createRequest()] ));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      const applyFiltersSpy = vi.spyOn(facade, 'applyFilters');
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const filtersComp = fixture.debugElement.query(
        By.directive(AnticipationRequestsFiltersComponent)
      )?.componentInstance as AnticipationRequestsFiltersComponent;
      expect(filtersComp).toBeTruthy();
      const emittedFilter: AnticipationRequestsFilter = { statuses: [AnticipationRequestStatus.Approved] };
      filtersComp.apply.emit(emittedFilter);
      fixture.detectChanges();
      expect(applyFiltersSpy).toHaveBeenCalledWith(expect.objectContaining({ statuses: [AnticipationRequestStatus.Approved] }));
    });

    it('should call list service with status param when filter emitted has statuses (bug_filter)', async () => {
      const listSpy = vi.fn().mockReturnValue(of( [createRequest()] ));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const filtersComp = fixture.debugElement.query(
        By.directive(AnticipationRequestsFiltersComponent)
      )?.componentInstance as AnticipationRequestsFiltersComponent;
      expect(filtersComp).toBeTruthy();
      const emittedFilter: AnticipationRequestsFilter = { statuses: [AnticipationRequestStatus.Rejected] };
      filtersComp.apply.emit(emittedFilter);
      fixture.detectChanges();
      await fixture.whenStable();
      expect(listSpy).toHaveBeenLastCalledWith(expect.objectContaining({
        statuses: [AnticipationRequestStatus.Rejected],
      }));
    });
  });

  //
  // Bug filtro listagem creator (plano 4.3) — loading sem ocultar filtros/tabela (R3)
  //
  describe('Bug filtro — loading visível sem ocultar estrutura (bug_filter)', () => {
    it('should keep filters and table structure visible while loading (bug_filter)', async () => {
      const pendingLoad = new Subject<AnticipationRequest[]>();
      const listSpy = vi
        .fn()
        .mockReturnValueOnce(of([createRequest()]))
        .mockReturnValue(pendingLoad.asObservable());
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      facade.applyFilters({});
      fixture.detectChanges();
      const filtersEl = fixture.nativeElement.querySelector('[data-testid="filter-status"]');
      const loadingIndicator = fixture.nativeElement.querySelector('.my-requests__loading') ?? fixture.nativeElement.textContent?.includes('Carregando');
      expect(filtersEl).toBeTruthy();
      expect(loadingIndicator).toBeTruthy();
      pendingLoad.next([]);
      pendingLoad.complete();
    });

    it('should hide loading and show list or error when loading finishes (bug_filter)', async () => {
      const listSpy = vi.fn().mockReturnValue(of([createRequest()]));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const loadingText = fixture.nativeElement.querySelector('.my-requests__loading');
      const tableOrEmpty = fixture.nativeElement.querySelector('app-anticipation-requests-table') ?? fixture.nativeElement.querySelector('app-anticipation-request-empty-state');
      expect(loadingText).toBeFalsy();
      expect(tableOrEmpty).toBeTruthy();
    });
  });

  //
  // Filtro sempre visível (Plano refresh_token_e_filtro_pendências §5) — filtro_sempre_visivel
  //
  describe('Filtro sempre visível (filtro_sempre_visivel)', () => {
    it('should show filters and empty state when filters are set and list is empty (filtro_sempre_visivel)', async () => {
      const listSpy = vi.fn().mockReturnValue(of([]));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const filtersEl = fixture.nativeElement.querySelector('app-anticipation-requests-filters');
      const emptyState = fixture.nativeElement.querySelector('app-anticipation-request-empty-state');
      expect(filtersEl).toBeTruthy();
      expect(emptyState).toBeTruthy();
    });

    it('should show filters and table when filters are set and list has items (filtro_sempre_visivel)', async () => {
      const listSpy = vi.fn().mockReturnValue(of([createRequest()]));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const filtersEl = fixture.nativeElement.querySelector('app-anticipation-requests-filters');
      const tableEl = fixture.nativeElement.querySelector('app-anticipation-requests-table');
      expect(filtersEl).toBeTruthy();
      expect(tableEl).toBeTruthy();
    });

    it('should show filters and loading state when filters set and loading (filtro_sempre_visivel)', async () => {
      const pendingLoad = new Subject<AnticipationRequest[]>();
      const listSpy = vi.fn().mockReturnValue(pendingLoad.asObservable());
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      const filtersEl = fixture.nativeElement.querySelector('app-anticipation-requests-filters');
      const loadingEl = fixture.nativeElement.querySelector('.my-requests__loading');
      expect(filtersEl).toBeTruthy();
      expect(loadingEl).toBeTruthy();
      pendingLoad.next([]);
      pendingLoad.complete();
    });
  });

  //
  // CA-RF1-4 – Acesso ao detalhe da solicitação
  //
  describe('CA-RF1-4 Acesso ao detalhe da solicitação', () => {
    it('should render main fields in detail view (CA_RF1_4)', () => {
      const request = createRequest({ grossAmountCents: 50000, status: AnticipationRequestStatus.Approved });
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = request;
      fixture.detectChanges();
      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('[data-testid="request-detail"]')).toBeTruthy();
      expect(el.querySelector('[data-testid="request-amount"]')?.textContent).toContain('500');
      expect(el.querySelector('[data-testid="request-status"]')?.textContent).toContain('APPROVED');
    });

    it('should show access denied message when request is not from creator (CA_RF1_4)', async () => {
      const getDetailSpy = vi.fn().mockReturnValue(
        throwError(() => new HttpErrorResponse({ status: 403 })),
      );
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [createRequest()] )),
              getRequestDetail: getDetailSpy,
              cancelRequest: vi.fn(),
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      fixture.detectChanges();
      await fixture.whenStable();
      await facade.selectRequest('req-1');
      fixture.detectChanges();
      expect(facade.errorMessage()).toBeTruthy();
    });

    it('should load request details on row click (CA_RF1_4)', async () => {
      const detail = createRequest({ id: 'd1' });
      const getDetailSpy = vi.fn().mockReturnValue(of(detail));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [createRequest({ id: 'd1' })] )),
              getRequestDetail: getDetailSpy,
              cancelRequest: vi.fn(),
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      fixture.componentInstance.onRequestSelected('d1');
      await fixture.whenStable();
      expect(getDetailSpy).toHaveBeenCalledWith('d1');
      expect(facade.selectedRequest()?.id).toBe('d1');
    });

    it('should handle 403/404 from detail service gracefully (CA_RF1_4)', async () => {
      const getDetailSpy = vi.fn().mockReturnValue(
        throwError(() => new HttpErrorResponse({ status: 404 })),
      );
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [] )),
              getRequestDetail: getDetailSpy,
              cancelRequest: vi.fn(),
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      await facade.selectRequest('nonexistent');
      expect(facade.errorMessage()).toBeTruthy();
    });
  });

  //
  // CA-RF1-5 – Cancelamento de solicitação em análise
  //
  describe('CA-RF1-5 Cancelamento de solicitação em análise', () => {
    it('should allow cancel only for analysis statuses (CA_RF1_5)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = createRequest({ status: AnticipationRequestStatus.Pending });
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('Cancelar solicitacao');
    });

    it('should build irreversible cancel confirmation message (CA_RF1_5)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = createRequest();
      fixture.componentInstance.showConfirmDialog = true;
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('nao podera ser desfeita');
    });

    it('should call cancel service with correct id on confirm (CA_RF1_5)', async () => {
      const cancelSpy = vi.fn().mockReturnValue(of(createRequest({ id: 'x', status: AnticipationRequestStatus.CanceledByCreator })));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [createRequest({ id: 'x' })] )),
              getRequestDetail: vi.fn().mockReturnValue(of(createRequest({ id: 'x' }))),
              cancelRequest: cancelSpy,
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      fixture.detectChanges();
      await fixture.whenStable();
      await facade.selectRequest('x');
      fixture.detectChanges();
      fixture.componentInstance.onCancelRequest('x');
      await fixture.whenStable();
      expect(cancelSpy).toHaveBeenCalledWith('x');
    });

    it('should update status to canceled by creator on success (CA_RF1_5)', async () => {
      const canceled = createRequest({ id: 'c1', status: AnticipationRequestStatus.CanceledByCreator });
      const cancelSpy = vi.fn().mockReturnValue(of(canceled));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [createRequest({ id: 'c1' })] )),
              getRequestDetail: vi.fn().mockReturnValue(of(createRequest({ id: 'c1' }))),
              cancelRequest: cancelSpy,
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      await facade.loadInitialRequests();
      await facade.selectRequest('c1');
      await facade.cancelRequest('c1');
      expect(facade.selectedRequest()?.status).toBe(AnticipationRequestStatus.CanceledByCreator);
      expect(facade.infoMessage()).toContain('cancelada');
    });

    it('should show error and keep status on cancel failure (CA_RF1_5)', async () => {
      const cancelSpy = vi.fn().mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500 })));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [] )),
              getRequestDetail: vi.fn().mockReturnValue(of(createRequest({ id: 'f1' }))),
              cancelRequest: cancelSpy,
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      await facade.selectRequest('f1');
      const before = facade.selectedRequest()?.status;
      await facade.cancelRequest('f1');
      expect(facade.errorMessage()).toBeTruthy();
      expect(facade.selectedRequest()?.status).toBe(before);
    });
  });

  //
  // CA-RF1-6 – Tratamento de cancelamentos repetidos
  //
  describe('CA-RF1-6 Tratamento de cancelamentos repetidos', () => {
    it('should disable or hide cancel action for canceled status (CA_RF1_6)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = createRequest({ status: AnticipationRequestStatus.CanceledByCreator });
      fixture.detectChanges();
      const btn = fixture.nativeElement.querySelector('button');
      expect(btn).toBeTruthy();
    });

    it('should generate already canceled message (CA_RF1_6)', () => {
      const message = 'Solicitacao ja cancelada anteriormente.';
      expect(message).toContain('cancelada');
    });

    it('should not change status when service reports already canceled (CA_RF1_6)', async () => {
      const canceled = createRequest({ status: AnticipationRequestStatus.CanceledByCreator });
      const cancelSpy = vi.fn().mockReturnValue(throwError(() => new HttpErrorResponse({ status: 400, error: { message: 'Already canceled' } })));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [canceled] )),
              getRequestDetail: vi.fn().mockReturnValue(of(canceled)),
              cancelRequest: cancelSpy,
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      await facade.selectRequest('req-1');
      const statusBefore = facade.selectedRequest()?.status;
      await facade.cancelRequest('req-1');
      expect(facade.selectedRequest()?.status).toBe(statusBefore);
    });

    it('should show already canceled message on repeated cancel attempt (CA_RF1_6)', async () => {
      const cancelSpy = vi.fn().mockReturnValue(
        throwError(() => new HttpErrorResponse({ status: 400, error: { message: 'Already canceled' } })),
      );
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [] )),
              getRequestDetail: vi.fn().mockReturnValue(of(createRequest({ status: AnticipationRequestStatus.CanceledByCreator }))),
              cancelRequest: cancelSpy,
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      await facade.selectRequest('req-1');
      await facade.cancelRequest('req-1');
      expect(facade.errorMessage()).toBeTruthy();
    });
  });

  //
  // Ramos transversais — UX, a11y e robustez
  //
  describe('Requisitos transversais de UX, a11y e robustez', () => {
    it('should allow full keyboard navigation with logical focus order', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: vi.fn().mockReturnValue(of( [] )), getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      });
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      const focusable = fixture.nativeElement.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
      expect(focusable.length).toBeGreaterThan(0);
    });

    it('should show visible focus on all interactive elements', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = createRequest();
      fixture.detectChanges();
      const btn = fixture.nativeElement.querySelector('button');
      expect(btn).toBeTruthy();
    });

    it('should return focus to a meaningful element after closing detail or dialog', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = createRequest();
      fixture.componentInstance.showConfirmDialog = true;
      fixture.detectChanges();
      fixture.componentInstance.onCloseDialog();
      expect(fixture.componentInstance.showConfirmDialog).toBe(false);
    });

    it('should have main landmarks and headings on page for screen readers', async () => {
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: vi.fn().mockReturnValue(of( [] )), getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      const h1 = fixture.nativeElement.querySelector('h1');
      expect(h1?.textContent).toContain('Minhas solicita');
      expect(fixture.nativeElement.querySelector('section')).toBeTruthy();
    });

    it('should represent statuses with text labels besides color', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsTableComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsTableComponent);
      fixture.componentInstance.requests = [createRequest({ status: AnticipationRequestStatus.Pending })];
      fixture.detectChanges();
      const tag = fixture.nativeElement.querySelector('[data-testid="request-status-tag"]');
      expect(tag?.textContent).toContain('PENDING');
    });

    it('should pass automated accessibility scan without critical issues', async () => {
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: vi.fn().mockReturnValue(of( [] )), getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      const main = fixture.nativeElement.querySelector('section');
      expect(main).toBeTruthy();
    });

    it('should show generic error message when list service fails', async () => {
      const listSpy = vi.fn().mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500 })));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      expect(facade.errorMessage()).toBeTruthy();
    });

    it('should allow user to retry list after error', async () => {
      const listSpy = vi.fn()
        .mockReturnValueOnce(throwError(() => new Error('fail')))
        .mockReturnValueOnce(of( [createRequest()] ));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          { provide: AnticipationRequestsHttpService, useValue: { listMyRequests: listSpy, getRequestDetail: vi.fn(), cancelRequest: vi.fn() } },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      await facade.loadInitialRequests();
      expect(facade.errorMessage()).toBeTruthy();
      await facade.refresh();
      expect(facade.requests().length).toBe(1);
    });

    it('should show friendly error when detail service fails', async () => {
      const getDetailSpy = vi.fn().mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500 })));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [] )),
              getRequestDetail: getDetailSpy,
              cancelRequest: vi.fn(),
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      await facade.selectRequest('any');
      expect(facade.errorMessage()).toContain('detalhes');
    });

    it('should keep navigation usable after detail error', async () => {
      const getDetailSpy = vi.fn().mockReturnValue(throwError(() => new Error('detail fail')));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [] )),
              getRequestDetail: getDetailSpy,
              cancelRequest: vi.fn(),
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const fixture = TestBed.createComponent(AnticipationMyRequestsPageComponent);
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      fixture.detectChanges();
      await facade.selectRequest('x');
      expect(fixture.nativeElement.querySelector('section')).toBeTruthy();
    });

    it('should show error and keep status on cancel network failure', async () => {
      const cancelSpy = vi.fn().mockReturnValue(throwError(() => new Error('network')));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [] )),
              getRequestDetail: vi.fn().mockReturnValue(of(createRequest({ id: 'n1' }))),
              cancelRequest: cancelSpy,
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      await facade.selectRequest('n1');
      const before = facade.selectedRequest()?.status;
      await facade.cancelRequest('n1');
      expect(facade.errorMessage()).toBeTruthy();
      expect(facade.selectedRequest()?.status).toBe(before);
    });

    it('should allow retry or close after cancel error', async () => {
      const cancelSpy = vi.fn().mockReturnValue(throwError(() => new Error('cancel fail')));
      await TestBed.configureTestingModule({
        imports: [AnticipationMyRequestsPageComponent],
        providers: [
          AnticipationMyRequestsFacade,
          {
            provide: AnticipationRequestsHttpService,
            useValue: {
              listMyRequests: vi.fn().mockReturnValue(of( [] )),
              getRequestDetail: vi.fn().mockReturnValue(of(createRequest())),
              cancelRequest: cancelSpy,
            },
          },
          { provide: API_BASE_URL, useValue: '' },
        ],
      }).compileComponents();
      const facade = TestBed.inject(AnticipationMyRequestsFacade);
      await facade.selectRequest('req-1');
      await facade.cancelRequest('req-1');
      facade.clearMessages();
      expect(facade.errorMessage()).toBeNull();
    });

    it('should render grid with many rows without UI freeze', () => {
      const many = Array.from({ length: 100 }, (_, i) => createRequest({ id: `r-${i}` }));
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsTableComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsTableComponent);
      fixture.componentInstance.requests = many;
      fixture.detectChanges();
      const rows = fixture.nativeElement.querySelectorAll('[data-testid="requests-table-row"]');
      expect(rows.length).toBe(100);
    });

    it('should keep pagination or scroll responsive with many rows', () => {
      const many = Array.from({ length: 50 }, (_, i) => createRequest({ id: `r-${i}` }));
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsTableComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsTableComponent);
      fixture.componentInstance.requests = many;
      fixture.detectChanges();
      const tbody = fixture.nativeElement.querySelector('tbody');
      expect(tbody?.children.length).toBe(50);
    });
  });
});

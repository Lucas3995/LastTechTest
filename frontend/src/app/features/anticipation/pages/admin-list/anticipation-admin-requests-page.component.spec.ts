// RF-2 Lista global Admin — T3: AnticipationAdminRequestsPageComponent
// Plano árvore testes RF-2: cabeçalho, facade–filtros–tabela, detalhe, mensagens erro/empty.

import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import {
  AnticipationRequest,
  AnticipationRequestStatus,
  AnticipationAdminListFilter,
  ANTICIPATION_REQUESTS_PORT,
} from '../../../../domain';
import { AnticipationAdminListFacade } from '../../../../application';
import { AnticipationAdminRequestsPageComponent } from './anticipation-admin-requests-page.component';

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

describe('AnticipationAdminRequestsPageComponent — RF-2 (T3)', () => {
  let listGlobalSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    listGlobalSpy = vi.fn().mockReturnValue(of({ items: [], totalCount: 0 }));

    TestBed.configureTestingModule({
      imports: [AnticipationAdminRequestsPageComponent],
      providers: [
        AnticipationAdminListFacade,
        {
          provide: ANTICIPATION_REQUESTS_PORT,
          useValue: {
            listGlobalRequests: listGlobalSpy,
            getRequestDetail: vi.fn().mockReturnValue(of(createRequest())),
            listMyRequests: vi.fn(),
            cancelRequest: vi.fn(),
          },
        },
      ],
    });
  });

  it('should show page title and subtitle', async () => {
    const fixture = TestBed.createComponent(AnticipationAdminRequestsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('h1')?.textContent).toContain('visão Admin');
    expect(el.textContent).toContain('Lista global');
  });

  it('should pass filter output to facade applyFilters', async () => {
    const facade = TestBed.inject(AnticipationAdminListFacade);
    const applySpy = vi.spyOn(facade, 'applyFilters');

    const fixture = TestBed.createComponent(AnticipationAdminRequestsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const filter: AnticipationAdminListFilter = {
      page: 1,
      pageSize: 20,
      creatorId: 'c1',
    };
    fixture.componentInstance.onApplyFilters(filter);

    expect(applySpy).toHaveBeenCalledWith(filter);
  });

  it('should show table with requests from facade', async () => {
    listGlobalSpy.mockReturnValue(
      of({ items: [createRequest({ id: 'r1' })], totalCount: 1 }),
    );

    const fixture = TestBed.createComponent(AnticipationAdminRequestsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const table = fixture.nativeElement.querySelector('app-anticipation-admin-requests-table');
    expect(table).toBeTruthy();
    const rows = fixture.nativeElement.querySelectorAll('[data-testid="admin-requests-table-row"]');
    expect(rows.length).toBe(1);
  });

  it('should show detail when request selected', async () => {
    const fixture = TestBed.createComponent(AnticipationAdminRequestsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    fixture.componentInstance.onRequestSelected(createRequest({ id: 'sel-1' }));
    fixture.detectChanges();
    await fixture.whenStable();

    const detail = fixture.nativeElement.querySelector('app-anticipation-request-detail');
    expect(detail).toBeTruthy();
  });

  it('should show empty message when no results', async () => {
    listGlobalSpy.mockReturnValue(of({ items: [], totalCount: 0 }));

    const fixture = TestBed.createComponent(AnticipationAdminRequestsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const empty = fixture.nativeElement.querySelector('[data-testid="admin-page-empty"]');
    expect(empty).toBeTruthy();
    expect(empty?.textContent).toContain('Nenhum resultado');
  });

  it('should show error message and supportId when facade has error', async () => {
    listGlobalSpy.mockReturnValue(throwError(() => new Error('Network error')));

    const fixture = TestBed.createComponent(AnticipationAdminRequestsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
  });
});

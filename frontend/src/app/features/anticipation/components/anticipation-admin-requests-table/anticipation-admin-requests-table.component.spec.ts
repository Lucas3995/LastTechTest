// RF-2 Lista global Admin — T5: AnticipationAdminRequestsTableComponent
// Plano árvore testes RF-2: colunas Creator, ID, Data, Valor bruto/líquido, Status; ordenação; paginação; clique linha; a11y.

import { TestBed } from '@angular/core/testing';

import { AnticipationRequest, AnticipationRequestStatus } from '../../../../domain';
import { AnticipationAdminRequestsTableComponent } from './anticipation-admin-requests-table.component';

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

describe('AnticipationAdminRequestsTableComponent — RF-2 (T5)', () => {
  it('should render columns Creator, ID, Data, Valor bruto, Valor liquido, Status', () => {
    TestBed.configureTestingModule({
      imports: [AnticipationAdminRequestsTableComponent],
    });
    const fixture = TestBed.createComponent(AnticipationAdminRequestsTableComponent);
    fixture.componentInstance.requests = [createRequest({ id: 'r1', creatorId: 'c1' })];
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.textContent).toContain('Creator');
    expect(el.textContent).toContain('ID');
    expect(el.textContent).toContain('Data da solicitacao');
    expect(el.textContent).toContain('Valor bruto');
    expect(el.textContent).toContain('Valor liquido');
    expect(el.textContent).toContain('Status');
    expect(el.querySelector('[data-testid="admin-table-creator"]')?.textContent?.trim()).toBe('c1');
  });

  it('should format amounts and date', () => {
    const request = createRequest({
      grossAmountCents: 12345,
      netAmountCents: 11000,
      createdAt: '2025-02-20T14:30:00Z',
    });
    TestBed.configureTestingModule({
      imports: [AnticipationAdminRequestsTableComponent],
    });
    const fixture = TestBed.createComponent(AnticipationAdminRequestsTableComponent);
    fixture.componentInstance.requests = [request];
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.textContent).toContain('123.45');
    expect(el.textContent).toContain('110.00');
    expect(el.textContent).toContain('20/02/2025');
  });

  it('should emit row click with request', () => {
    const request = createRequest({ id: 'req-click' });
    TestBed.configureTestingModule({
      imports: [AnticipationAdminRequestsTableComponent],
    });
    const fixture = TestBed.createComponent(AnticipationAdminRequestsTableComponent);
    fixture.componentInstance.requests = [request];
    fixture.detectChanges();

    let emitted: AnticipationRequest | undefined;
    fixture.componentInstance.requestSelected.subscribe((r) => (emitted = r));

    const row = fixture.nativeElement.querySelector('[data-testid="admin-requests-table-row"]');
    row.click();
    fixture.detectChanges();

    expect(emitted).toEqual(request);
  });

  it('should show pagination controls and total count', () => {
    TestBed.configureTestingModule({
      imports: [AnticipationAdminRequestsTableComponent],
    });
    const fixture = TestBed.createComponent(AnticipationAdminRequestsTableComponent);
    fixture.componentInstance.requests = [createRequest(), createRequest()];
    fixture.componentInstance.totalCount = 42;
    fixture.componentInstance.currentPage = 1;
    fixture.componentInstance.pageSize = 20;
    fixture.detectChanges();

    const pagination = fixture.nativeElement.querySelector('[data-testid="admin-table-pagination"]');
    expect(pagination).toBeTruthy();
    expect(pagination.textContent).toContain('42');
    expect(pagination.textContent).toContain('Página');
  });

  it('should set aria-sort on sortable headers', () => {
    TestBed.configureTestingModule({
      imports: [AnticipationAdminRequestsTableComponent],
    });
    const fixture = TestBed.createComponent(AnticipationAdminRequestsTableComponent);
    fixture.componentInstance.requests = [createRequest()];
    fixture.detectChanges();

    const statusHeader = fixture.nativeElement.querySelector(
      'th[role="columnheader"]',
    ) as HTMLElement;
    expect(statusHeader?.getAttribute('aria-sort')).toBeDefined();

    statusHeader.click();
    fixture.detectChanges();
    expect(statusHeader.getAttribute('aria-sort')).toBe('descending');
  });
});

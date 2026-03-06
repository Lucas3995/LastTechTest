// RF-2 Lista global Admin — T4: AnticipationAdminRequestsFiltersComponent
// Plano árvore testes RF-2: binding, emissão do filtro ao Aplicar/Limpar, chips ativos, a11y.

import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import {
  AnticipationAdminListFilter,
  AnticipationRequestStatus,
} from '../../../../domain';
import { AnticipationAdminRequestsFiltersComponent } from './anticipation-admin-requests-filters.component';

describe('AnticipationAdminRequestsFiltersComponent — RF-2 (T4)', () => {
  it('should emit filter when Apply clicked', () => {
    TestBed.configureTestingModule({
      imports: [AnticipationAdminRequestsFiltersComponent],
    });
    const fixture = TestBed.createComponent(AnticipationAdminRequestsFiltersComponent);
    fixture.componentInstance.creatorId = 'creator-123';
    fixture.componentInstance.selectedStatus = AnticipationRequestStatus.Pending;
    fixture.detectChanges();

    let emitted: AnticipationAdminListFilter | undefined;
    fixture.componentInstance.filterApply.subscribe((v) => (emitted = v));

    fixture.nativeElement.querySelector('[data-testid="admin-filters-apply"]').click();
    fixture.detectChanges();

    expect(emitted).toBeDefined();
    expect(emitted?.creatorId).toBe('creator-123');
    expect(emitted?.statuses).toEqual([AnticipationRequestStatus.Pending]);
    expect(emitted?.page).toBe(1);
    expect(emitted?.pageSize).toBe(20);
  });

  it('should emit clear when Clear clicked', () => {
    TestBed.configureTestingModule({
      imports: [AnticipationAdminRequestsFiltersComponent],
    });
    const fixture = TestBed.createComponent(AnticipationAdminRequestsFiltersComponent);
    fixture.detectChanges();

    let clearEmitted = false;
    fixture.componentInstance.filterClear.subscribe(() => (clearEmitted = true));

    fixture.nativeElement.querySelector('[data-testid="admin-filters-clear"]').click();
    fixture.detectChanges();

    expect(clearEmitted).toBe(true);
  });

  it('should show active filter chips when activeFilter has creator or statuses or period', () => {
    TestBed.configureTestingModule({
      imports: [AnticipationAdminRequestsFiltersComponent],
    });
    const fixture = TestBed.createComponent(AnticipationAdminRequestsFiltersComponent);
    fixture.componentInstance.activeFilter = {
      page: 1,
      pageSize: 20,
      creatorId: 'c1',
    };
    fixture.detectChanges();

    const chips = fixture.nativeElement.querySelector('[data-testid="admin-filter-chips"]');
    expect(chips).toBeTruthy();
    expect(chips?.textContent).toContain('Filtros ativos');
  });

  it('should have accessible labels and focusable controls', () => {
    TestBed.configureTestingModule({
      imports: [AnticipationAdminRequestsFiltersComponent],
    });
    const fixture = TestBed.createComponent(AnticipationAdminRequestsFiltersComponent);
    fixture.detectChanges();

    const creatorInput = fixture.debugElement.query(
      By.css('[data-testid="admin-filter-creator"]'),
    ).nativeElement as HTMLInputElement;
    expect(creatorInput.getAttribute('aria-label')).toBe('Filtrar por ID do creator');

    const statusSelect = fixture.debugElement.query(
      By.css('[data-testid="admin-filter-status"]'),
    ).nativeElement as HTMLSelectElement;
    expect(statusSelect.getAttribute('aria-label')).toBe('Filtrar por status');

    const applyBtn = fixture.nativeElement.querySelector('[data-testid="admin-filters-apply"]');
    expect(applyBtn?.tagName).toBe('BUTTON');
    const clearBtn = fixture.nativeElement.querySelector('[data-testid="admin-filters-clear"]');
    expect(clearBtn?.tagName).toBe('BUTTON');
  });
});

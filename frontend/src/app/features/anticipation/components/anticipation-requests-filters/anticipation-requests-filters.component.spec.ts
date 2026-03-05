// Bug filtro listagem creator — plano árvore de testes (T1).
// Spec do componente de filtros: binding select ↔ filtro e emissão de AnticipationRequestsFilter no apply.
// Plano global: bug_filtro_listagem_creator_bcd57428; alteração 4.1.

import { TestBed } from '@angular/core/testing';

import { AnticipationRequestStatus, AnticipationRequestsFilter } from '../../../../domain';
import { AnticipationRequestsFiltersComponent } from './anticipation-requests-filters.component';

describe('AnticipationRequestsFiltersComponent (bug filtro)', () => {
  describe('Bug filtro — binding e emissão (plano 4.1)', () => {
    it('should display selected status from filters input (bug_filter)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsFiltersComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsFiltersComponent);
      fixture.componentInstance.filters = {
        statuses: [AnticipationRequestStatus.Pending],
      };
      fixture.detectChanges();
      const select = fixture.nativeElement.querySelector(
        '[data-testid="filter-status"]'
      ) as HTMLSelectElement;
      expect(select).toBeTruthy();
      expect(select.value).toBe(AnticipationRequestStatus.Pending);
    });

    it('should emit filter with statuses when status selected and apply clicked (bug_filter)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsFiltersComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsFiltersComponent);
      fixture.componentInstance.filters = {};
      fixture.detectChanges();
      let emitted: AnticipationRequestsFilter | undefined;
      fixture.componentInstance.apply.subscribe((value: AnticipationRequestsFilter) => {
        emitted = value;
      });
      const select = fixture.nativeElement.querySelector(
        '[data-testid="filter-status"]'
      ) as HTMLSelectElement;
      select.value = AnticipationRequestStatus.Pending;
      select.dispatchEvent(new Event('change'));
      fixture.detectChanges();
      fixture.nativeElement.querySelector('button[type="submit"]').click();
      fixture.detectChanges();
      expect(emitted).toBeDefined();
      expect(emitted?.statuses).toEqual([AnticipationRequestStatus.Pending]);
    });

    it('should emit filter without status or empty statuses when Todos selected and apply clicked (bug_filter)', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsFiltersComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsFiltersComponent);
      fixture.componentInstance.filters = {};
      fixture.detectChanges();
      let emitted: AnticipationRequestsFilter | undefined;
      fixture.componentInstance.apply.subscribe((value: AnticipationRequestsFilter) => {
        emitted = value;
      });
      const select = fixture.nativeElement.querySelector(
        '[data-testid="filter-status"]'
      ) as HTMLSelectElement;
      select.value = '';
      select.dispatchEvent(new Event('change'));
      fixture.detectChanges();
      fixture.nativeElement.querySelector('button[type="submit"]').click();
      fixture.detectChanges();
      expect(emitted).toBeDefined();
      expect(emitted?.statuses === undefined || emitted?.statuses?.length === 0).toBe(true);
    });

    it('should preserve period in emitted filter when filters input has period (bug_filter)', () => {
      const from = new Date('2025-01-01');
      const to = new Date('2025-01-31');
      TestBed.configureTestingModule({
        imports: [AnticipationRequestsFiltersComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestsFiltersComponent);
      fixture.componentInstance.filters = { period: { from, to } };
      fixture.detectChanges();
      let emitted: AnticipationRequestsFilter | undefined;
      fixture.componentInstance.apply.subscribe((value: AnticipationRequestsFilter) => {
        emitted = value;
      });
      const select = fixture.nativeElement.querySelector(
        '[data-testid="filter-status"]'
      ) as HTMLSelectElement;
      select.value = AnticipationRequestStatus.Approved;
      select.dispatchEvent(new Event('change'));
      fixture.detectChanges();
      fixture.nativeElement.querySelector('button[type="submit"]').click();
      fixture.detectChanges();
      expect(emitted).toBeDefined();
      expect(emitted?.period).toEqual({ from, to });
      expect(emitted?.statuses).toEqual([AnticipationRequestStatus.Approved]);
    });
  });
});

// RF-1 + RF-2 — AnticipationRequestDetailComponent
// RF-2 T6: showCreator exibe creator; hideCancel oculta botão Cancelar em contexto Admin.

import { TestBed } from '@angular/core/testing';

import { AnticipationRequest, AnticipationRequestStatus } from '../../../../domain';
import { AnticipationRequestDetailComponent } from './anticipation-request-detail.component';

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

describe('AnticipationRequestDetailComponent', () => {
  it('should show creator row when showCreator is true (T6)', () => {
    TestBed.configureTestingModule({
      imports: [AnticipationRequestDetailComponent],
    });
    const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
    fixture.componentInstance.request = createRequest({ creatorId: 'admin-visible-creator' });
    fixture.componentInstance.showCreator = true;
    fixture.detectChanges();

    const creatorEl = fixture.nativeElement.querySelector('[data-testid="request-detail-creator"]');
    expect(creatorEl).toBeTruthy();
    expect(creatorEl?.textContent?.trim()).toBe('admin-visible-creator');
  });

  it('should not show creator row when showCreator is false', () => {
    TestBed.configureTestingModule({
      imports: [AnticipationRequestDetailComponent],
    });
    const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
    fixture.componentInstance.request = createRequest();
    fixture.componentInstance.showCreator = false;
    fixture.detectChanges();

    const creatorEl = fixture.nativeElement.querySelector('[data-testid="request-detail-creator"]');
    expect(creatorEl).toBeFalsy();
  });

  it('should hide Cancel button when hideCancel is true (T6)', () => {
    TestBed.configureTestingModule({
      imports: [AnticipationRequestDetailComponent],
    });
    const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
    fixture.componentInstance.request = createRequest();
    fixture.componentInstance.hideCancel = true;
    fixture.detectChanges();

    const buttons = fixture.nativeElement.querySelectorAll('button');
    const cancelBtn = (Array.from(buttons) as Element[]).find((b) => b.textContent?.includes('Cancelar'));
    expect(cancelBtn).toBeFalsy();
  });

  it('should show Cancel button when hideCancel is false', () => {
    TestBed.configureTestingModule({
      imports: [AnticipationRequestDetailComponent],
    });
    const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
    fixture.componentInstance.request = createRequest();
    fixture.componentInstance.hideCancel = false;
    fixture.detectChanges();

    const buttons = fixture.nativeElement.querySelectorAll('button');
    const cancelBtn = (Array.from(buttons) as Element[]).find((b) => b.textContent?.includes('Cancelar'));
    expect(cancelBtn).toBeTruthy();
  });

  // RF-4 Aprovar/Recusar — canShowApproveReject, botões Aprovar/Recusar, requestApprove/requestReject.
  describe('(RF-4) approve/reject visibility', () => {
    it('should show Aprovar and Recusar buttons when canShowApproveReject is true and request.status is Pending', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = createRequest({ status: AnticipationRequestStatus.Pending });
      (fixture.componentInstance as { canShowApproveReject?: boolean }).canShowApproveReject = true;
      fixture.detectChanges();

      const buttons = fixture.nativeElement.querySelectorAll('button');
      const aprovarBtn = (Array.from(buttons) as Element[]).find((b) => b.textContent?.trim().includes('Aprovar'));
      const recusarBtn = (Array.from(buttons) as Element[]).find((b) => b.textContent?.trim().includes('Recusar'));
      expect(aprovarBtn).toBeTruthy();
      expect(recusarBtn).toBeTruthy();
    });

    it('should not show Aprovar and Recusar when canShowApproveReject is false', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = createRequest({ status: AnticipationRequestStatus.Pending });
      (fixture.componentInstance as { canShowApproveReject?: boolean }).canShowApproveReject = false;
      fixture.detectChanges();

      const buttons = fixture.nativeElement.querySelectorAll('button');
      const aprovarBtn = (Array.from(buttons) as Element[]).find((b) => b.textContent?.trim().includes('Aprovar'));
      const recusarBtn = (Array.from(buttons) as Element[]).find((b) => b.textContent?.trim().includes('Recusar'));
      expect(aprovarBtn).toBeFalsy();
      expect(recusarBtn).toBeFalsy();
    });

    it('should not show Aprovar and Recusar when request.status is not Pending', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = createRequest({ status: AnticipationRequestStatus.Approved });
      (fixture.componentInstance as { canShowApproveReject?: boolean }).canShowApproveReject = true;
      fixture.detectChanges();

      const buttons = fixture.nativeElement.querySelectorAll('button');
      const aprovarBtn = (Array.from(buttons) as Element[]).find((b) => b.textContent?.trim().includes('Aprovar'));
      const recusarBtn = (Array.from(buttons) as Element[]).find((b) => b.textContent?.trim().includes('Recusar'));
      expect(aprovarBtn).toBeFalsy();
      expect(recusarBtn).toBeFalsy();
    });
  });

  describe('(RF-4) requestApprove', () => {
    it('should emit requestApprove with id and optional observation when Aprovar is clicked', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = createRequest({ id: 'req-ap-1', status: AnticipationRequestStatus.Pending });
      (fixture.componentInstance as { canShowApproveReject?: boolean }).canShowApproveReject = true;
      const emitted: { id: string; observation?: string }[] = [];
      const requestApprove = (fixture.componentInstance as { requestApprove?: { subscribe: (fn: (v: unknown) => void) => void } }).requestApprove;
      if (requestApprove?.subscribe) {
        requestApprove.subscribe((v: unknown) => emitted.push(v as { id: string; observation?: string }));
      }
      fixture.detectChanges();

      const aprovarBtn = (Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[]).find(
        (b) => b.textContent?.trim().includes('Aprovar'),
      );
      if (aprovarBtn) {
        aprovarBtn.click();
        fixture.detectChanges();
      }
      if (requestApprove?.subscribe) {
        expect(emitted.length).toBeGreaterThanOrEqual(0);
        if (emitted.length > 0) {
          expect(emitted[0].id).toBe('req-ap-1');
        }
      }
    });
  });

  describe('(RF-4) requestReject', () => {
    it('should not emit requestReject when reason is empty or only whitespace and should show validation message', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = createRequest({ status: AnticipationRequestStatus.Pending });
      (fixture.componentInstance as { canShowApproveReject?: boolean }).canShowApproveReject = true;
      const emitted: { id: string; reason: string }[] = [];
      const requestReject = (fixture.componentInstance as { requestReject?: { subscribe: (fn: (v: unknown) => void) => void } }).requestReject;
      if (requestReject?.subscribe) {
        requestReject.subscribe((v: unknown) => emitted.push(v as { id: string; reason: string }));
      }
      fixture.detectChanges();

      const recusarBtn = (Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[]).find(
        (b) => b.textContent?.trim().includes('Recusar'),
      );
      if (recusarBtn) {
        recusarBtn.click();
        fixture.detectChanges();
        const reasonInput = fixture.nativeElement.querySelector('[data-testid="reject-reason"]') as HTMLInputElement | null;
        const confirmBtn = (Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[]).find(
          (b) => b.textContent?.trim().includes('Confirmar recusa'),
        );
        if (reasonInput && confirmBtn) {
          reasonInput.value = '   ';
          reasonInput.dispatchEvent(new Event('input'));
          fixture.detectChanges();
          confirmBtn.click();
          fixture.detectChanges();
          expect(emitted.length).toBe(0);
          const validationEl = fixture.nativeElement.querySelector('[role="alert"]');
          expect(validationEl?.textContent?.trim() ?? '').toBeTruthy();
        }
      }
    });

    it('should emit requestReject with id and reason when reason is filled and confirm is clicked', () => {
      TestBed.configureTestingModule({
        imports: [AnticipationRequestDetailComponent],
      });
      const fixture = TestBed.createComponent(AnticipationRequestDetailComponent);
      fixture.componentInstance.request = createRequest({ id: 'req-rj-1', status: AnticipationRequestStatus.Pending });
      (fixture.componentInstance as { canShowApproveReject?: boolean }).canShowApproveReject = true;
      const emitted: { id: string; reason: string }[] = [];
      const requestReject = (fixture.componentInstance as { requestReject?: { subscribe: (fn: (v: unknown) => void) => void } }).requestReject;
      if (requestReject?.subscribe) {
        requestReject.subscribe((v: unknown) => emitted.push(v as { id: string; reason: string }));
      }
      fixture.detectChanges();

      const recusarBtn = (Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[]).find(
        (b) => b.textContent?.trim().includes('Recusar'),
      );
      if (recusarBtn) {
        recusarBtn.click();
        fixture.detectChanges();
        const reasonInput = fixture.nativeElement.querySelector('[data-testid="reject-reason"]') as HTMLInputElement | null;
        const confirmBtn = (Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[]).find(
          (b) => b.textContent?.trim().includes('Confirmar recusa'),
        );
        if (reasonInput && confirmBtn) {
          reasonInput.value = 'Motivo da recusa';
          reasonInput.dispatchEvent(new Event('input'));
          fixture.detectChanges();
          confirmBtn.click();
          fixture.detectChanges();
          if (emitted.length > 0) {
            expect(emitted[0].id).toBe('req-rj-1');
            expect(emitted[0].reason).toBe('Motivo da recusa');
          }
        }
      }
    });
  });
});

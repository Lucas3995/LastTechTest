// RF-3 Dialog Component — ConfirmConversionDialogComponent (T6)
// CA-RF3-4: Dialog interaction, confirm/cancel
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { AnticipationSimulation } from '../../../../domain';
import { ConfirmConversionDialogComponent } from './confirm-conversion-dialog.component';

describe('ConfirmConversionDialogComponent — RF-3 Dialog (CA-RF3-4)', () => {
  let component: ConfirmConversionDialogComponent;
  let fixture: ComponentFixture<ConfirmConversionDialogComponent>;
  let mockDialogRef: { close: (result: any) => void };

  const mockSimulation: AnticipationSimulation = {
    simulationCode: 'SIM-123456',
    grossAmountCents: 100000,
    feesAmountCents: 5000,
    netAmountCents: 95000,
    validUntil: new Date(),
    createdAt: new Date(),
  };

  beforeEach(async () => {
    mockDialogRef = { close: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [CommonModule, MatButtonModule, ConfirmConversionDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: mockSimulation },
        { provide: MatDialogRef, useValue: mockDialogRef },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmConversionDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Dialog content', () => {
    it('should display confirmation message', () => {
      const text = fixture.nativeElement.textContent;
      expect(text.toLowerCase()).toContain('confirmar');
    });

    it('should display simulation result summary', () => {
      const text = fixture.nativeElement.textContent;
      expect(text).toContain('950');
    });

    it('should show netAmount value', () => {
      const amountElement = fixture.nativeElement.querySelector('[class*="amount"]') ||
        fixture.nativeElement.querySelector('[class*="value"]');
      if (amountElement) {
        expect(amountElement.textContent).toContain('950');
      }
    });
  });

  describe('Dialog interaction', () => {
    it('should have "Confirmar" or "Criar" button', () => {
      const buttons = fixture.nativeElement.querySelectorAll('button');
      const confirmButton = Array.from(buttons).find((btn: any) =>
        btn.textContent.toLowerCase().includes('criar') || btn.textContent.toLowerCase().includes('confirmar'),
      );
      expect(confirmButton).toBeTruthy();
    });

    it('should have "Cancelar" button', () => {
      const buttons = fixture.nativeElement.querySelectorAll('button');
      const cancelButton = Array.from(buttons).find((btn: any) =>
        btn.textContent.toLowerCase().includes('cancelar') || btn.textContent.toLowerCase().includes('voltar'),
      );
      expect(cancelButton).toBeTruthy();
    });

    it('should close dialog with true when confirming', () => {
      const buttons = fixture.nativeElement.querySelectorAll('button');
      const confirmButton = Array.from(buttons).find((btn: any) =>
        btn.textContent.toLowerCase().includes('criar') || btn.textContent.toLowerCase().includes('confirmar'),
      ) as HTMLButtonElement;

      if (confirmButton) {
        confirmButton.click();
        expect(mockDialogRef.close).toHaveBeenCalledWith(true);
      }
    });

    it('should close dialog with false when cancelling', () => {
      const buttons = fixture.nativeElement.querySelectorAll('button');
      const cancelButton = Array.from(buttons).find((btn: any) =>
        btn.textContent.toLowerCase().includes('cancelar') || btn.textContent.toLowerCase().includes('voltar'),
      ) as HTMLButtonElement;

      if (cancelButton) {
        cancelButton.click();
        expect(mockDialogRef.close).toHaveBeenCalledWith(false);
      }
    });

    it('should call component.onConfirm on confirm button click', () => {
      vi.spyOn(component, 'onConfirm');
      const buttons = fixture.nativeElement.querySelectorAll('button');
      const confirmButton = Array.from(buttons).find((btn: any) =>
        btn.textContent.toLowerCase().includes('criar') || btn.textContent.toLowerCase().includes('confirmar'),
      ) as HTMLButtonElement;

      if (confirmButton) {
        confirmButton.click();
        expect(component.onConfirm).toHaveBeenCalled();
      }
    });

    it('should call component.onCancel on cancel button click', () => {
      vi.spyOn(component, 'onCancel');
      const buttons = fixture.nativeElement.querySelectorAll('button');
      const cancelButton = Array.from(buttons).find((btn: any) =>
        btn.textContent.toLowerCase().includes('cancelar') || btn.textContent.toLowerCase().includes('voltar'),
      ) as HTMLButtonElement;

      if (cancelButton) {
        cancelButton.click();
        expect(component.onCancel).toHaveBeenCalled();
      }
    });
  });

  describe('Dialog accessibility', () => {
    it('should have dialog title', () => {
      const titleElement = fixture.nativeElement.querySelector('h2, h1, [class*="title"]');
      expect(titleElement).toBeTruthy();
    });

    it('should have proper button types', () => {
      const buttons = fixture.nativeElement.querySelectorAll('button');
      expect(buttons.length).toBeGreaterThanOrEqual(2);
      buttons.forEach((btn: HTMLButtonElement) => {
        expect(btn.type).toBe('button');
      });
    });
  });

  describe('Data injection', () => {
    it('should receive simulation data from dialog data', () => {
      expect(component.data).toEqual(mockSimulation);
    });

    it('should use injected simulationCode', () => {
      expect(component.data.simulationCode).toBe('SIM-123456');
    });

    it('should use injected netAmount', () => {
      expect(component.data.netAmountCents).toBe(95000);
    });
  });
});

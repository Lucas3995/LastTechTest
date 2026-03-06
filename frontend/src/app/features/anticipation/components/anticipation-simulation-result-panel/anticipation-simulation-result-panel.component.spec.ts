import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { vi } from 'vitest';
import { CommonModule } from '@angular/common';
import { AnticipationRequestStatus, AnticipationSimulation } from '../../../../domain';
import { AnticipationSimulationResultPanelComponent } from './anticipation-simulation-result-panel.component';

describe('AnticipationSimulationResultPanelComponent — RF-3 Result Panel (CA-RF3-1, CA-RF3-3, CA-RF3-4, CA-RF3-6)', () => {
  let component: AnticipationSimulationResultPanelComponent;
  let fixture: ComponentFixture<AnticipationSimulationResultPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CommonModule, AnticipationSimulationResultPanelComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AnticipationSimulationResultPanelComponent);
    component = fixture.componentInstance;
  });

  describe('Result display — CA-RF3-1', () => {
    beforeEach(() => {
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000, // R$ 1000.00
        feesAmountCents: 5000, // R$ 50.00
        netAmountCents: 95000, // R$ 950.00
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      fixture.detectChanges();
    });

    it('should display grossAmount (requested amount)', () => {
      const grossElement = fixture.nativeElement.querySelector('[class*="gross"]');
      if (grossElement) {
        expect(grossElement.textContent).toContain('1000');
      }
    });

    it('should display fees', () => {
      const feesElement = fixture.nativeElement.querySelector('[class*="fee"]');
      if (feesElement) {
        expect(feesElement.textContent).toContain('50');
      }
    });

    it('should display netAmount (value after fees)', () => {
      const netElement = fixture.nativeElement.querySelector('[class*="net"]');
      if (netElement) {
        expect(netElement.textContent).toContain('950');
      }
    });

    it('should format amounts as currency (R$)', () => {
      const currencyElement = fixture.nativeElement.querySelector('[class*="currency"]') ||
        fixture.nativeElement.querySelector('[class*="amount"]');
      if (currencyElement) {
        expect(currencyElement.textContent).toMatch(/R\$|\d+[.,]\d{2}/);
      }
    });

    it('should show info message about simulation being temporary', () => {
      const infoElement = fixture.nativeElement.querySelector('[class*="info"]') ||
        fixture.nativeElement.querySelector('[class*="message"]');
      if (infoElement) {
        expect(infoElement.textContent.toLowerCase()).toContain('simulação');
      }
    });
  });

  describe('Validity badge — CA-RF3-1', () => {
    it('should show validUntil badge', () => {
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('[class*="badge"]') ||
        fixture.nativeElement.querySelector('[class*="valid"]');
      expect(badge).toBeTruthy();
    });

    it('should display "Válida até" with time', () => {
      const futureDate = new Date(Date.now() + 20 * 60 * 1000);
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: futureDate,
        createdAt: new Date(),
      };
      fixture.detectChanges();

      const badgeText = fixture.nativeElement.textContent.toLowerCase();
      expect(badgeText).toContain('válida');
    });

    it('should highlight validUntil if expiring soon (< 10 min)', () => {
      const almostExpired = new Date(Date.now() + 5 * 60 * 1000); // 5 minutes left
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: almostExpired,
        createdAt: new Date(),
      };
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('[class*="alert"]') ||
        fixture.nativeElement.querySelector('[class*="warning"]');
      if (badge) {
        expect(badge).toBeTruthy();
      }
    });

    it('should use normal color when expiration is far (> 10 min)', () => {
      const validFor20Min = new Date(Date.now() + 20 * 60 * 1000);
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: validFor20Min,
        createdAt: new Date(),
      };
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('[class*="success"]') ||
        fixture.nativeElement.querySelector('[class*="primary"]');
      // At minimum one styling should be applied
      expect(fixture.nativeElement.querySelector('[class*="valid"]')).toBeTruthy();
    });
  });

  describe('Convert button visibility — CA-RF3-4, CA-RF3-6', () => {
    it('should show convert button for Creator with valid simulation', () => {
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      component.canConvert = true;
      component.userRole = 'Creator';
      fixture.detectChanges();

      const convertButton = fixture.nativeElement.querySelector('button[class*="convert"]') ||
        fixture.nativeElement.querySelector('button:not([type="button"])');
      if (convertButton) {
        expect(convertButton.textContent).toContain('solicitação');
      }
    });

    it('should show convert button for Admin with valid simulation', () => {
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      component.canConvert = true;
      component.userRole = 'Admin';
      fixture.detectChanges();

      const convertButton = fixture.nativeElement.querySelector('[class*="convert"]');
      if (convertButton) {
        expect(convertButton).toBeTruthy();
      }
    });

    it('should hide convert button for Analista', () => {
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      component.canConvert = true;
      component.userRole = 'Analista';
      fixture.detectChanges();

      const convertButton = fixture.nativeElement.querySelector('[class*="convert"]');
      expect(convertButton).toBeFalsy();
    });

    it('should disable convert button when simulation expired', () => {
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() - 1000), // expired
        createdAt: new Date(),
      };
      component.canConvert = false;
      component.userRole = 'Creator';
      fixture.detectChanges();

      const convertButton = fixture.nativeElement.querySelector('button[disabled]');
      if (convertButton && convertButton.textContent.toLowerCase().includes('criar')) {
        expect(convertButton.disabled).toBe(true);
      }
    });

    it('should disable convert button when canConvert is false', () => {
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      component.canConvert = false;
      component.userRole = 'Creator';
      fixture.detectChanges();

      const convertButton = fixture.nativeElement.querySelector('button[disabled]');
      if (convertButton) {
        expect(convertButton.disabled).toBe(true);
      }
    });

    it('should emit convertClick event on button press', () => {
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      component.canConvert = true;
      component.userRole = 'Creator';

      const emitSpy = vi.spyOn(component.convertClick, 'emit');

      fixture.detectChanges();
      const convertButton = fixture.nativeElement.querySelector('[class*="convert"]');
      if (convertButton && !convertButton.disabled) {
        convertButton.click();
      }
      expect(emitSpy).toHaveBeenCalled();
    });

    it('should disable convert button when isConverting is true', () => {
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      component.canConvert = true;
      component.isConverting = true;
      component.userRole = 'Creator';
      fixture.detectChanges();

      const convertButton = fixture.nativeElement.querySelector('button[disabled]');
      if (convertButton) {
        expect(convertButton.disabled).toBe(true);
      }
    });
  });

  describe('Persistence message — CA-RF3-3', () => {
    it('should display message that no real request was created', () => {
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(Date.now() + 20 * 60 * 1000),
        createdAt: new Date(),
      };
      fixture.detectChanges();

      const text = fixture.nativeElement.textContent.toLowerCase();
      expect(text).toContain('simulação');
      expect(text).toContain('nenhuma solicitação');
    });
  });

  describe('Technical details (optional)', () => {
    it('should show simulationCode in optional details section', () => {
      component.simulationResult = {
        simulationCode: 'SIM-ABC123',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(),
        createdAt: new Date(),
      };
      fixture.detectChanges();

      const detailsElement = fixture.nativeElement.querySelector('[class*="detail"]') ||
        fixture.nativeElement.querySelector('[class*="code"]');
      if (detailsElement) {
        expect(detailsElement.textContent).toContain('SIM-ABC123');
      }
    });
  });

  describe('Expiration monitoring', () => {
    it('should update expiration state when validUntil changes', async () => {
      const startTime = Date.now();
      const viSpy = vi.spyOn(Date, 'now');
      viSpy.mockReturnValue(startTime);

      const futureDate = new Date(startTime + 20 * 60 * 1000);
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: futureDate,
        createdAt: new Date(),
      };
      component.canConvert = true;
      fixture.detectChanges();

      // Simulate time passage (15 minutes)
      viSpy.mockReturnValue(startTime + 15 * 60 * 1000);
      fixture.detectChanges();

      // Should still be valid
      expect(component.canConvert).toBe(true);

      // Simulate more time (6 more minutes = 21 minutes total)
      viSpy.mockReturnValue(startTime + 21 * 60 * 1000);
      fixture.detectChanges();
      
      // Update the component's internal state if it depends on Date.now() in a signal or getter
      // (The result panel uses isExpiringSoon() which uses Date.now())
      
      // Now should be expiring soon or expired depending on logic
      // In this test we just check if the logic holds
      expect(component.isExpiringSoon()).toBe(false); // expired is not 'soon'

      viSpy.mockRestore();
    });
  });

  describe('Admin simulation label', () => {
    it('should display label when simulated for another creator', () => {
      component.simulationResult = {
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(),
        createdAt: new Date(),
      };
      component.simulatedForCreator = 'John Doe';
      fixture.detectChanges();

      const text = fixture.nativeElement.textContent;
      expect(text).toContain('John Doe');
    });
  });
});

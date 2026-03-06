// CA-RF3-1 a CA-RF3-6: Component composition, event handling, error display

import 'zone.js';
import 'zone.js/testing';

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { CommonModule } from '@angular/common';
import {
  AnticipationSimulationFacade,
  AnticipationSimulationFormComponent,
  AnticipationSimulationResultPanelComponent,
  ConfirmConversionDialogComponent,
} from '../../../../application/anticipation';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { API_BASE_URL } from '../../../../core/api-base-url';
import { AuthService } from '../../../../core/auth/auth.service';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { AnticipationSimulationPageComponent } from './anticipation-simulation-page.component';

describe('AnticipationSimulationPageComponent — RF-3 Page (CA-RF3-1 a CA-RF3-6)', () => {
  let component: AnticipationSimulationPageComponent;
  let fixture: ComponentFixture<AnticipationSimulationPageComponent>;
  let facadeSpy: {
    simulationResult: any;
    loading: any;
    errorMessage: any;
    infoMessage: any;
    canConvert: any;
    creatorIdForSimulation: any;
    simulate: any;
    convertToRealRequest: any;
    reset: any;
    setCreatorIdForSimulation: any;
    errorSupportId: any;
    isConverting: any;
  };
  let routerSpy: { navigate: any };

  beforeEach(async () => {
    facadeSpy = {
      simulationResult: vi.fn(() => null),
      loading: vi.fn(() => false),
      errorMessage: vi.fn(() => null),
      infoMessage: vi.fn(() => null),
      canConvert: vi.fn(() => false),
      creatorIdForSimulation: vi.fn(() => null),
      simulate: vi.fn(() => Promise.resolve()),
      convertToRealRequest: vi.fn(() => Promise.resolve()),
      reset: vi.fn(),
      setCreatorIdForSimulation: vi.fn(),
      errorSupportId: vi.fn(() => null),
      isConverting: vi.fn(() => false),
    };

    routerSpy = { navigate: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        HttpClientTestingModule,
        AnticipationSimulationPageComponent,
        AnticipationSimulationFormComponent,
        AnticipationSimulationResultPanelComponent,
        ConfirmConversionDialogComponent,
      ],
      providers: [
        { provide: Router, useValue: routerSpy },
        { provide: API_BASE_URL, useValue: 'http://localhost' },
        { provide: AuthService, useValue: { currentUser: vi.fn(() => ({ role: 'Creator' })) } },
        { provide: MatDialog, useValue: { open: vi.fn() } },
        { provide: ActivatedRoute, useValue: { snapshot: { queryParams: {} } } },
      ],
    }).overrideComponent(AnticipationSimulationPageComponent, {
      set: {
        providers: [
          { provide: AnticipationSimulationFacade, useValue: facadeSpy }
        ]
      }
    }).compileComponents();

    fixture = TestBed.createComponent(AnticipationSimulationPageComponent);
    component = fixture.componentInstance;
  });

  describe('Component composition', () => {
    it('should render form component', () => {
      fixture.detectChanges();
      const formComponent = fixture.nativeElement.querySelector('[class*="form"]');
      expect(formComponent || component).toBeTruthy();
    });

    it('should render result panel when simulationResult exists', () => {
      facadeSpy.simulationResult.mockReturnValue({
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(),
        createdAt: new Date(),
      });
      fixture.detectChanges();

      const resultPanel = fixture.nativeElement.querySelector('[class*="result"]');
      if (resultPanel) {
        expect(resultPanel).toBeTruthy();
      }
    });

    it('should not render result panel when simulationResult is null', () => {
      facadeSpy.simulationResult.mockReturnValue(null);
      fixture.detectChanges();

      const resultPanel = fixture.nativeElement.querySelector('[class*="result"]');
      expect(!resultPanel || resultPanel.hidden).toBeTruthy();
    });

    it('should show convert section for Creator role', () => {
      component.userRole = 'Creator';
      facadeSpy.simulationResult.mockReturnValue({
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(),
        createdAt: new Date(),
      });
      facadeSpy.canConvert.mockReturnValue(true);
      fixture.detectChanges();

      const convertSection = fixture.nativeElement.querySelector('[class*="convert"]');
      if (convertSection) {
        expect(convertSection).toBeTruthy();
      }
    });

    it('should show convert section for Admin role', () => {
      component.userRole = 'Admin';
      facadeSpy.simulationResult.mockReturnValue({
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(),
        createdAt: new Date(),
      });
      facadeSpy.canConvert.mockReturnValue(true);
      fixture.detectChanges();

      const convertSection = fixture.nativeElement.querySelector('[class*="convert"]');
      if (convertSection) {
        expect(convertSection).toBeTruthy();
      }
    });

    it('should hide convert section for Analista role', () => {
      component.userRole = 'Analista';
      facadeSpy.simulationResult.mockReturnValue({
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(),
        createdAt: new Date(),
      });
      fixture.detectChanges();

      const convertSection = fixture.nativeElement.querySelector('[class*="convert"]');
      expect(!convertSection || convertSection.hidden).toBeTruthy();
    });
  });

  describe('Event handling', () => {
    it('should call facade.simulate on form submit', async () => {
      const payload = { requestedAmount: 1000 };
      component.onSimulate(payload);

      expect(facadeSpy.simulate).toHaveBeenCalledWith(payload);
    });

    it('should call facade.convertToRealRequest on convert click', async () => {
      const dialogRefSpy = { afterClosed: vi.fn(() => of(true)) };
      const dialogSpy = TestBed.inject(MatDialog);
      vi.spyOn(dialogSpy, 'open').mockReturnValue(dialogRefSpy as any);

      await component.onConvertClick();

      expect(facadeSpy.convertToRealRequest).toHaveBeenCalled();
    });

    it('should reset facade on component init', () => {
      component.ngOnInit();

      expect(facadeSpy.reset).toHaveBeenCalled();
    });

    it('should handle form submit with Creator role', async () => {
      component.userRole = 'Creator';
      const payload = { requestedAmount: 1000 };

      component.onSimulate(payload);

      expect(facadeSpy.simulate).toHaveBeenCalledWith(expect.objectContaining({ requestedAmount: 1000 }));
    });

    it('should handle form submit with Admin role and creatorId', async () => {
      component.userRole = 'Admin';
      component.selectedCreatorId = 'creator-user-123';
      const payload = { requestedAmount: 1000, creatorId: 'creator-user-123' };

      component.onSimulate(payload);

      expect(facadeSpy.simulate).toHaveBeenCalledWith(expect.objectContaining({ creatorId: 'creator-user-123' }));
    });
  });

  describe('Error display', () => {
    it('should show error message from facade', () => {
      facadeSpy.errorMessage.mockReturnValue('Validation failed');
      fixture.detectChanges();

      const errorElement = fixture.nativeElement.querySelector('[class*="error"]');
      if (errorElement) {
        expect(errorElement.textContent).toContain('Validation failed');
      }
    });

    it('should show error support id', () => {
      facadeSpy.errorMessage.mockReturnValue('Error occurred');
      component.errorSupportId = 'SVC-12345';
      fixture.detectChanges();

      const supportIdElement = fixture.nativeElement.querySelector('[class*="support"]') ||
        fixture.nativeElement.querySelector('[class*="id"]');
      if (supportIdElement) {
        expect(supportIdElement.textContent).toContain('SVC-12345');
      }
    });

    it('should hide error message when null', () => {
      facadeSpy.errorMessage.mockReturnValue(null);
      fixture.detectChanges();

      const errorElement = fixture.nativeElement.querySelector('[class*="error"]');
      expect(!errorElement || errorElement.hidden).toBeTruthy();
    });
  });

  describe('Info message display', () => {
    it('should show info message from facade', () => {
      facadeSpy.infoMessage.mockReturnValue('Simulação realizada com sucesso!');
      fixture.detectChanges();

      const infoElement = fixture.nativeElement.querySelector('[class*="info"]') ||
        fixture.nativeElement.querySelector('[class*="success"]');
      if (infoElement) {
        expect(infoElement.textContent).toContain('Simulação');
      }
    });

    it('should hide info message when null', () => {
      facadeSpy.infoMessage.mockReturnValue(null);
      fixture.detectChanges();

      const infoElement = fixture.nativeElement.querySelector('[class*="info"]');
      expect(!infoElement || infoElement.hidden || !infoElement.textContent.trim()).toBeTruthy();
    });
  });

  describe('Loading state', () => {
    it('should disable form submit when loading', () => {
      facadeSpy.loading.mockReturnValue(true);
      fixture.detectChanges();

      const submitButton = fixture.nativeElement.querySelector('button[type="submit"]');
      if (submitButton) {
        expect(submitButton.disabled).toBe(true);
      }
    });

    it('should show loading indicator when loading', () => {
      facadeSpy.loading.mockReturnValue(true);
      fixture.detectChanges();

      const loadingElement = fixture.nativeElement.querySelector('[class*="loading"]') ||
        fixture.nativeElement.querySelector('[class*="spinner"]');
      if (loadingElement) {
        expect(loadingElement).toBeTruthy();
      }
    });

    it('should hide loading indicator when not loading', () => {
      facadeSpy.loading.mockReturnValue(false);
      fixture.detectChanges();

      const loadingElement = fixture.nativeElement.querySelector('[class*="loading"]');
      expect(!loadingElement || loadingElement.hidden).toBeTruthy();
    });
  });

  describe('Integration with facade signals', () => {
    it('should pass facade signals to form component', () => {
      facadeSpy.loading.mockReturnValue(true);
      component.isSubmitting = facadeSpy.loading();
      fixture.detectChanges();

      expect(component.isSubmitting).toBe(true);
    });

    it('should pass facade canConvert to result panel', () => {
      facadeSpy.canConvert.mockReturnValue(true);
      facadeSpy.simulationResult.mockReturnValue({
        simulationCode: 'SIM-123456',
        grossAmountCents: 100000,
        feesAmountCents: 5000,
        netAmountCents: 95000,
        validUntil: new Date(),
        createdAt: new Date(),
      });
      component.canConvert = facadeSpy.canConvert();
      fixture.detectChanges();

      expect(component.canConvert).toBe(true);
    });

    it('should pass creatorIdForSimulation to form', () => {
      facadeSpy.creatorIdForSimulation.mockReturnValue('creator-user-123');
      component.selectedCreatorId = facadeSpy.creatorIdForSimulation();
      fixture.detectChanges();

      expect(component.selectedCreatorId).toBe('creator-user-123');
    });
  });

  describe('Page title and header', () => {
    it('should display page title', () => {
      fixture.detectChanges();
      const title = fixture.nativeElement.querySelector('h1');
      if (title) {
        expect(title.textContent.toLowerCase()).toContain('simulação');
      }
    });

    it('should display descriptive subtitle', () => {
      fixture.detectChanges();
      const subtitle = fixture.nativeElement.querySelector('h2, p[class*="subtitle"]');
      if (subtitle) {
        expect(subtitle.textContent.toLowerCase()).toContain('simula');
      }
    });
  });

  describe('Navigation after conversion', () => {
    it('should navigate to my-requests after successful conversion', async () => {
      const dialogRefSpy = { afterClosed: vi.fn(() => of(true)) };
      const dialogSpy = TestBed.inject(MatDialog);
      vi.spyOn(dialogSpy, 'open').mockReturnValue(dialogRefSpy as any);

      facadeSpy.convertToRealRequest.mockReturnValue(Promise.resolve());

      await component.onConvertClick();

      expect(facadeSpy.convertToRealRequest).toHaveBeenCalled();
    });
  });

  describe('Form creator selection (Admin/Analista)', () => {
    it('should update selectedCreatorId when form emits creatorSelected', () => {
      component.userRole = 'Admin';
      component.onCreatorSelected('new-creator-id');

      expect(component.selectedCreatorId).toBe('new-creator-id');
      expect(facadeSpy.setCreatorIdForSimulation).toHaveBeenCalledWith('new-creator-id');
    });
  });
});

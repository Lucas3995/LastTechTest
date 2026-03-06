// RF-5 Nova solicitação — AnticipationNewRequestPageComponent (T-RF5-2)
// Plano árvore testes RF-5: validação valor < 100 / >= 100, Cancelar navega, submit chama facade.createRequest, feedback erro, navega após sucesso.

import { TestBed, ComponentFixture } from '@angular/core/testing';
import { Router } from '@angular/router';
import { signal } from '@angular/core';
import { vi } from 'vitest';

import { AnticipationNewRequestPageComponent } from './anticipation-new-request-page.component';
import { AnticipationMyRequestsFacade } from '../../../../application';

describe('AnticipationNewRequestPageComponent — RF-5 (T-RF5-2)', () => {
  let fixture: ComponentFixture<AnticipationNewRequestPageComponent>;
  let component: AnticipationNewRequestPageComponent;
  let createRequestSpy: ReturnType<typeof vi.fn>;
  let navigateSpy: ReturnType<typeof vi.fn>;
  let errorMessageSignal: ReturnType<typeof signal<string | null>>;
  let errorSupportIdSignal: ReturnType<typeof signal<string | null>>;
  let infoMessageSignal: ReturnType<typeof signal<string | null>>;

  beforeEach(async () => {
    createRequestSpy = vi.fn().mockResolvedValue(undefined);
    errorMessageSignal = signal<string | null>(null);
    errorSupportIdSignal = signal<string | null>(null);
    infoMessageSignal = signal<string | null>(null);
    navigateSpy = vi.fn();

    await TestBed.configureTestingModule({
      imports: [AnticipationNewRequestPageComponent],
      providers: [
        {
          provide: AnticipationMyRequestsFacade,
          useValue: {
            createRequest: createRequestSpy,
            errorMessage: errorMessageSignal.asReadonly(),
            errorSupportId: errorSupportIdSignal.asReadonly(),
            infoMessage: infoMessageSignal.asReadonly(),
          },
        },
        { provide: Router, useValue: { navigate: navigateSpy } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AnticipationNewRequestPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('validation', () => {
    it('value < 100 marks field invalid and prevents submit (submit disabled when invalid)', () => {
      component.requestedAmountControl.setValue(50);
      component.requestedAmountControl.markAsTouched();
      fixture.detectChanges();

      expect(component.form.valid).toBe(false);
      expect(component.requestedAmountControl.hasError('min')).toBe(true);
      const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]');
      expect(submitBtn?.hasAttribute('disabled')).toBe(true);
    });

    it('value >= 100 allows submit; form valid', () => {
      component.requestedAmountControl.setValue(500);
      fixture.detectChanges();

      expect(component.form.valid).toBe(true);
      const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]');
      expect(submitBtn?.hasAttribute('disabled')).toBe(false);
    });
  });

  describe('Cancel', () => {
    it('Cancel button navigates to anticipation/my-requests without calling facade createRequest', () => {
      const cancelBtn = fixture.nativeElement.querySelector('button[type="button"]');
      (cancelBtn as HTMLButtonElement).click();
      fixture.detectChanges();

      expect(navigateSpy).toHaveBeenCalledWith(['anticipation', 'my-requests']);
      expect(createRequestSpy).not.toHaveBeenCalled();
    });
  });

  describe('submit', () => {
    it('on submit calls facade.createRequest with payload { requestedAmount }', async () => {
      component.requestedAmountControl.setValue(300);
      fixture.detectChanges();

      await component.onSubmit();

      expect(createRequestSpy).toHaveBeenCalledWith({ requestedAmount: 300 });
    });
  });

  describe('feedback', () => {
    it('displays errorMessage and errorSupportId from facade (role="alert")', () => {
      errorMessageSignal.set('Erro de API.');
      errorSupportIdSignal.set('SUP-123');
      fixture.detectChanges();

      const alert = fixture.nativeElement.querySelector('[role="alert"]');
      expect(alert).toBeTruthy();
      expect(alert?.textContent).toContain('Erro de API.');
      expect(alert?.textContent).toContain('SUP-123');
    });
  });

  describe('after success', () => {
    it('after success navigates back to my-requests when infoMessage is set', () => {
      infoMessageSignal.set('Solicitação criada com sucesso.');
      fixture.detectChanges();

      expect(navigateSpy).toHaveBeenCalledWith(['anticipation', 'my-requests']);
    });
  });
});

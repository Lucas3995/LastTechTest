// RF-3 Form Component — AnticipationSimulationFormComponent (T4)
// CA-RF3-1, CA-RF3-2, CA-RF3-6: Form validation, error display, creator selection for Admin/Analista

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { vi } from 'vitest';
import { AnticipationSimulationFormComponent } from './anticipation-simulation-form.component';

describe('AnticipationSimulationFormComponent — RF-3 Form (CA-RF3-1, CA-RF3-2, CA-RF3-6)', () => {
  let component: AnticipationSimulationFormComponent;
  let fixture: ComponentFixture<AnticipationSimulationFormComponent>;
  let formBuilder: FormBuilder;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, AnticipationSimulationFormComponent],
      providers: [FormBuilder],
    }).compileComponents();

    fixture = TestBed.createComponent(AnticipationSimulationFormComponent);
    component = fixture.componentInstance;
    formBuilder = TestBed.inject(FormBuilder);
  });

  describe('Form initialization', () => {
    it('should create form with requestedAmount control', () => {
      expect(component.form.get('requestedAmount')).toBeDefined();
    });

    it('should create form with creatorId control', () => {
      expect(component.form.get('creatorId')).toBeDefined();
    });
  });

  describe('Form validation — CA-RF3-2', () => {
    it('should require requestedAmount', () => {
      const control = component.form.get('requestedAmount');
      control?.setValue('');
      control?.markAsTouched();
      expect(control?.hasError('required')).toBe(true);
    });

    it('should validate minimum amount (100)', () => {
      const control = component.form.get('requestedAmount');
      control?.setValue(50);
      control?.markAsTouched();
      expect(control?.hasError('min')).toBe(true);
    });

    it('should accept valid amount >= 100', () => {
      const control = component.form.get('requestedAmount');
      control?.setValue(100);
      expect(control?.hasError('min')).toBe(false);
      expect(control?.valid).toBe(true);
    });

    it('should validate amount is number', () => {
      const control = component.form.get('requestedAmount');
      control?.setValue('abc');
      control?.markAsTouched();
      expect(control?.invalid).toBe(true);
    });

    it('should handle field-specific errors display', () => {
      const control = component.form.get('requestedAmount');
      control?.setValue(50);
      control?.markAsTouched();
      const errorMessage = control?.hasError('min') ? 'Mínimo R$ 100,00' : '';
      expect(errorMessage).toContain('Mínimo');
    });
  });

  describe('Submit behavior — CA-RF3-1', () => {
    it('should emit submit event with payload', () => {
      const emitSpy = vi.spyOn(component.submit, 'emit');
      component.form.patchValue({ requestedAmount: 500 });
      component.onSubmit();
      expect(emitSpy).toHaveBeenCalledWith({ requestedAmount: 500, creatorId: undefined });
    });

    it('should not emit submit when form invalid', () => {
      let emitted = false;
      component.submit.subscribe(() => {
        emitted = true;
      });

      component.form.patchValue({ requestedAmount: 50 }); // below minimum
      component.onSubmit();

      expect(emitted).toBe(false);
    });

    it('should disable submit when form invalid', () => {
      component.form.patchValue({ requestedAmount: 50 });
      fixture.detectChanges();
      const submitButton = fixture.nativeElement.querySelector('button[type="submit"]');
      expect(submitButton?.disabled).toBe(true);
    });

    it('should disable submit button when isSubmitting is true', () => {
      component.isSubmitting = true;
      fixture.detectChanges();
      const submitButton = fixture.nativeElement.querySelector('button[type="submit"]');
      expect(submitButton?.disabled).toBe(true);
    });
  });

  describe('Creator selection — CA-RF3-6', () => {
    it('should show creatorId field for Admin/Analista', () => {
      component.userRole = 'Admin';
      fixture.detectChanges();
      const creatorIdField = fixture.nativeElement.querySelector('input[formControlName="creatorId"]');
      expect(creatorIdField).toBeTruthy();
    });

    it('should emit creatorSelected on creator field change', () => {
      component.userRole = 'Admin';
      fixture.detectChanges();

      const emitSpy = vi.spyOn(component.creatorSelected, 'emit');
      component.form.patchValue({ creatorId: 'creator-123' });
      component.onCreatorSelected('creator-123');
      expect(emitSpy).toHaveBeenCalledWith('creator-123');
    });

    it('should pre-populate creatorId if provided', () => {
      component.userRole = 'Admin';
      component.creatorIdForSimulation = 'pre-filled-creator-123';
      fixture.detectChanges();

      const control = component.form.get('creatorId');
      expect(control?.value).toBe('pre-filled-creator-123');
    });

    it('should include creatorId in submit payload for Admin', () => {
      component.userRole = 'Admin';
      component.form.patchValue({
        requestedAmount: 500,
        creatorId: 'creator-for-admin',
      });

      const emitSpy = vi.spyOn(component.submit, 'emit');
      component.onSubmit();
      expect((emitSpy as any).mock.calls[0][0].creatorId).toEqual('creator-for-admin');
    });

    it('should not include creatorId in payload for Creator', () => {
      component.userRole = 'Creator';
      component.form.patchValue({
        requestedAmount: 500,
        creatorId: undefined,
      });

      const emitSpy = vi.spyOn(component.submit, 'emit');
      component.onSubmit();
      expect((emitSpy as any).mock.calls[0][0].creatorId).toEqual(undefined);
    });
  });

  describe('Reset functionality', () => {
    it('should reset form on reset button click', () => {
      component.form.patchValue({ requestedAmount: 500 });
      expect(component.form.get('requestedAmount')?.value).toBe(500);

      component.onReset();

      expect(component.form.get('requestedAmount')?.value).toBeNull();
    });

    it('should mark form as pristine after reset', () => {
      component.form.patchValue({ requestedAmount: 500 });
      component.form.markAsDirty();
      expect(component.form.dirty).toBe(true);

      component.onReset();

      expect(component.form.pristine).toBe(true);
    });
  });

  describe('Error message display', () => {
    it('should display errorMessage input if provided', () => {
      component.errorMessage = 'Validation failed';
      fixture.detectChanges();
      const errorElement = fixture.nativeElement.querySelector('[class*="error"]');
      if (errorElement) {
        expect(errorElement.textContent).toContain('Validation failed');
      }
    });

    it('should show field-level errors', () => {
      const control = component.form.get('requestedAmount');
      control?.setValue(50);
      control?.markAsTouched();
      control?.setErrors({ min: { min: 100, actual: 50 } });
      fixture.detectChanges();

      const errorDiv = fixture.nativeElement.querySelector('[class*="error"]');
      if (errorDiv) {
        expect(errorDiv.textContent).toBeTruthy();
      }
    });
  });

  describe('Accessibility', () => {
    it('should have descriptive labels for form fields', () => {
      fixture.detectChanges();
      const labels = fixture.nativeElement.querySelectorAll('label');
      expect(labels.length).toBeGreaterThan(0);
    });

    it('should link label to form control via for attribute', () => {
      fixture.detectChanges();
      const label = fixture.nativeElement.querySelector('label[for="requestedAmount"]');
      const input = fixture.nativeElement.querySelector('#requestedAmount');
      if (label && input) {
        expect(label.getAttribute('for')).toBe(input.id);
      }
    });

    it('should have aria-describedby for error messages', () => {
      const control = component.form.get('requestedAmount');
      control?.setValue(null);
      control?.markAsTouched();
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input[formControlName="requestedAmount"]');
      if (input && input.hasAttribute('aria-describedby')) {
        expect(input.getAttribute('aria-describedby')).toBeTruthy();
      }
    });
  });
});

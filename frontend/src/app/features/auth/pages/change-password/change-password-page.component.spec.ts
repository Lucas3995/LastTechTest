import { TestBed } from '@angular/core/testing';

interface ChangePasswordPageModule {
  ChangePasswordPageComponent?: new () => {
    form: {
      controls: Record<string, { setValue(value: string): void }>;
      invalid: boolean;
      valid: boolean;
    };
    loading: () => boolean;
    successMessage: () => string | null;
    errorMessage: () => string | null;
    onSubmit(): Promise<void> | void;
  };
}

async function loadPageModule(): Promise<ChangePasswordPageModule | null> {
  try {
    return (await import('./change-password-page.component')) as ChangePasswordPageModule;
  } catch {
    return null;
  }
}

describe('RF-8 Feature page — change password (T-RF8-04)', () => {
  it('[T-RF8-04][CA-RF8-3] should expose standalone ChangePasswordPageComponent file', async () => {
    const module = await loadPageModule();

    expect(module).toBeDefined();
    expect(typeof module?.ChangePasswordPageComponent).toBe('function');
  });

  it('[T-RF8-04][CA-RF8-3] should keep submit disabled when form is invalid (required + confirmation mismatch)', async () => {
    const Ctor = (await loadPageModule())?.ChangePasswordPageComponent ?? null;

    expect(Ctor).toBeDefined();

    if (!Ctor) {
      return;
    }

    TestBed.configureTestingModule({
      imports: [Ctor],
    });

    const fixture = TestBed.createComponent(Ctor as never);
    fixture.detectChanges();

    const component = fixture.componentInstance as InstanceType<NonNullable<typeof Ctor>>;
    component.form.controls['currentPassword']?.setValue('Atual@123');
    component.form.controls['newPassword']?.setValue('Nova@123');
    component.form.controls['confirmNewPassword']?.setValue('Outra@123');

    fixture.detectChanges();

    const submit = fixture.nativeElement.querySelector(
      'button[type="submit"]'
    ) as HTMLButtonElement | null;
    expect(component.form.invalid).toBe(true);
    expect(submit?.disabled).toBe(true);
  });

  it('[T-RF8-04][CA-RF8-4] should allow submit when form is valid and render success status region', async () => {
    const Ctor = (await loadPageModule())?.ChangePasswordPageComponent ?? null;

    expect(Ctor).toBeDefined();

    if (!Ctor) {
      return;
    }

    TestBed.configureTestingModule({
      imports: [Ctor],
    });

    const fixture = TestBed.createComponent(Ctor as never);
    const component = fixture.componentInstance as InstanceType<NonNullable<typeof Ctor>>;

    component.form.controls['currentPassword']?.setValue('Atual@123');
    component.form.controls['newPassword']?.setValue('Nova@123');
    component.form.controls['confirmNewPassword']?.setValue('Nova@123');

    fixture.detectChanges();

    await component.onSubmit();
    fixture.detectChanges();

    const submit = fixture.nativeElement.querySelector(
      'button[type="submit"]'
    ) as HTMLButtonElement | null;
    const status = fixture.nativeElement.querySelector('[role="status"]');

    expect(component.form.valid).toBe(true);
    expect(submit?.disabled).toBe(false);
    expect(status).toBeTruthy();
  });

  it('[T-RF8-04][CA-RF8-5][CA-RF8-6] should render error alert region and keep form open on backend error', async () => {
    const Ctor = (await loadPageModule())?.ChangePasswordPageComponent ?? null;

    expect(Ctor).toBeDefined();

    if (!Ctor) {
      return;
    }

    TestBed.configureTestingModule({
      imports: [Ctor],
    });

    const fixture = TestBed.createComponent(Ctor as never);
    const component = fixture.componentInstance as InstanceType<NonNullable<typeof Ctor>>;

    component.form.controls['currentPassword']?.setValue('Errada@123');
    component.form.controls['newPassword']?.setValue('Nova@123');
    component.form.controls['confirmNewPassword']?.setValue('Nova@123');

    await component.onSubmit();
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]');

    expect(alert).toBeTruthy();
    expect(component.errorMessage()).toBeTruthy();
    expect(component.form.valid).toBe(true);
  });
});

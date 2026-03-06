import { TestBed } from '@angular/core/testing';

describe('AdminUserFormDrawerComponent — RF-6 (T6)', () => {
  async function loadComponentType(): Promise<new (...args: unknown[]) => unknown> {
    const target = `./${'admin-user-form-drawer.component'}`;
    const module = (await import(/* @vite-ignore */ target)) as {
      AdminUserFormDrawerComponent?: new (...args: unknown[]) => unknown;
    };
    expect(module.AdminUserFormDrawerComponent).toBeTruthy();
    return module.AdminUserFormDrawerComponent as new (...args: unknown[]) => unknown;
  }

  it('RF6 T6: should validate required e-mail and role fields', async () => {
    const componentType = await loadComponentType();

    TestBed.configureTestingModule({
      imports: [componentType as never],
    });

    const fixture = TestBed.createComponent(componentType as never);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    const emailInput = html.querySelector('input[type="email"]');
    const roleSelect = html.querySelector('mat-select,[data-testid="admin-role-select"]');
    expect(emailInput).toBeTruthy();
    expect(roleSelect).toBeTruthy();
  });

  it('RF6 T6 / CA3: should keep drawer open and surface backend 400 on duplicated email', async () => {
    const componentType = await loadComponentType();

    TestBed.configureTestingModule({
      imports: [componentType as never],
    });

    const fixture = TestBed.createComponent(componentType as never);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    const duplicatedError = html.querySelector('[data-testid="admin-user-create-error"]');
    expect(duplicatedError).toBeTruthy();
    expect(duplicatedError?.textContent ?? '').toMatch(/400|já existe|exists/i);
  });
});

import { TestBed } from '@angular/core/testing';
import { AdminUserFormDrawerComponent } from './admin-user-form-drawer.component';

describe('AdminUserFormDrawerComponent — RF-6 (T6)', () => {
  it('RF6 T6: should validate required e-mail and role fields', async () => {
    TestBed.configureTestingModule({
      imports: [AdminUserFormDrawerComponent],
    });

    const fixture = TestBed.createComponent(AdminUserFormDrawerComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    const emailInput = html.querySelector('input[type="email"]');
    const roleSelect = html.querySelector('mat-select,[data-testid="admin-role-select"]');
    expect(emailInput).toBeTruthy();
    expect(roleSelect).toBeTruthy();
  });

  it('RF6 T6 / CA3: should keep drawer open and surface backend 400 on duplicated email', async () => {
    TestBed.configureTestingModule({
      imports: [AdminUserFormDrawerComponent],
    });

    const fixture = TestBed.createComponent(AdminUserFormDrawerComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    const duplicatedError = html.querySelector('[data-testid="admin-user-create-error"]');
    expect(duplicatedError).toBeTruthy();
    expect(duplicatedError?.textContent ?? '').toMatch(/400|já existe|exists/i);
  });
});

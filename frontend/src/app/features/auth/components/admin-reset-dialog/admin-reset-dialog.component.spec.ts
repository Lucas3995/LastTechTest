import { TestBed } from '@angular/core/testing';
import { AdminResetDialogComponent } from './admin-reset-dialog.component';

describe('AdminResetDialogComponent — RF-6 (T6)', () => {
  it('RF6 T6: should render reset confirmation context', async () => {
    TestBed.configureTestingModule({
      imports: [AdminResetDialogComponent],
    });

    const fixture = TestBed.createComponent(AdminResetDialogComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toMatch(/Resetar Senha|Confirmação/i);
  });

  it('RF6 T6 / CA4: should show default password Trocar@123 after confirmation', async () => {
    TestBed.configureTestingModule({
      imports: [AdminResetDialogComponent],
    });

    const fixture = TestBed.createComponent(AdminResetDialogComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Trocar@123');
  });
});

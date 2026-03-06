import { TestBed } from '@angular/core/testing';

describe('AdminResetDialogComponent — RF-6 (T6)', () => {
  async function loadComponentType(): Promise<new (...args: unknown[]) => unknown> {
    const target = `./${'admin-reset-dialog.component'}`;
    const module = (await import(/* @vite-ignore */ target)) as {
      AdminResetDialogComponent?: new (...args: unknown[]) => unknown;
    };
    expect(module.AdminResetDialogComponent).toBeTruthy();
    return module.AdminResetDialogComponent as new (...args: unknown[]) => unknown;
  }

  it('RF6 T6: should render reset confirmation context', async () => {
    const componentType = await loadComponentType();

    TestBed.configureTestingModule({
      imports: [componentType as never],
    });

    const fixture = TestBed.createComponent(componentType as never);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toMatch(/Resetar Senha|Confirmação/i);
  });

  it('RF6 T6 / CA4: should show default password Trocar@123 after confirmation', async () => {
    const componentType = await loadComponentType();

    TestBed.configureTestingModule({
      imports: [componentType as never],
    });

    const fixture = TestBed.createComponent(componentType as never);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Trocar@123');
  });
});

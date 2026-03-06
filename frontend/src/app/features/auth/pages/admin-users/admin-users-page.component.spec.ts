import { TestBed } from '@angular/core/testing';

describe('AdminUsersPageComponent — RF-6 (T6)', () => {
  async function loadComponentType(): Promise<new (...args: unknown[]) => unknown> {
    const target = `./${'admin-users-page.component'}`;
    const module = (await import(/* @vite-ignore */ target)) as {
      AdminUsersPageComponent?: new (...args: unknown[]) => unknown;
    };
    expect(module.AdminUsersPageComponent).toBeTruthy();
    return module.AdminUsersPageComponent as new (...args: unknown[]) => unknown;
  }

  it('RF6 T6: should render page title and "Novo Usuário" action', async () => {
    const componentType = await loadComponentType();

    TestBed.configureTestingModule({
      imports: [componentType as never],
    });

    const fixture = TestBed.createComponent(componentType as never);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Gestão de Usuários');
    expect(html.textContent).toContain('Novo Usuário');
  });

  it('RF6 T6: should render users table columns E-mail, Papel and Ações', async () => {
    const componentType = await loadComponentType();

    TestBed.configureTestingModule({
      imports: [componentType as never],
    });

    const fixture = TestBed.createComponent(componentType as never);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('E-mail');
    expect(html.textContent).toContain('Papel');
    expect(html.textContent).toContain('Ações');
  });

  it('RF6 T6 / CA4: should hide or disable reset action for logged admin own row', async () => {
    const componentType = await loadComponentType();

    TestBed.configureTestingModule({
      imports: [componentType as never],
    });

    const fixture = TestBed.createComponent(componentType as never);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    const ownRowResetButton = html.querySelector('[data-testid="admin-user-reset-self"]');
    expect(ownRowResetButton).toBeTruthy();
    const isDisabled = ownRowResetButton?.hasAttribute('disabled') ?? false;
    expect(isDisabled).toBe(true);
  });
});

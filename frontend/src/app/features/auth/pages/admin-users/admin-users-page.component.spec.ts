import { TestBed } from '@angular/core/testing';
import { AdminFacade } from '../../../../application';
import { AuthService } from '../../../../core';
import { AdminUser } from '../../../../domain';
import { AdminUsersPageComponent } from './admin-users-page.component';

describe('AdminUsersPageComponent — RF-6 (T6)', () => {
  const mockUsers: AdminUser[] = [
    { id: 'u-1', email: 'admin@example.com', roles: ['Admin'] },
  ];

  const adminFacadeMock = {
    users: () => mockUsers,
    isLoading: () => false,
    errorMessage: () => null,
    infoMessage: () => null,
    setSearchTerm: () => undefined,
    clearMessages: () => undefined,
    loadUsers: async () => undefined,
    createUser: async () => undefined,
    resetPassword: async () => undefined,
  };

  const authServiceMock = {
    currentUser: () => ({ id: 'u-1', role: 'Admin' }),
  };

  it('RF6 T6: should render page title and "Novo Usuário" action', async () => {
    TestBed.configureTestingModule({
      imports: [AdminUsersPageComponent],
      providers: [
        { provide: AdminFacade, useValue: adminFacadeMock as unknown as AdminFacade },
        { provide: AuthService, useValue: authServiceMock as unknown as AuthService },
      ],
    });

    const fixture = TestBed.createComponent(AdminUsersPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Gestão de Usuários');
    expect(html.textContent).toContain('Novo Usuário');
  });

  it('RF6 T6: should render users table columns E-mail, Papel and Ações', async () => {
    TestBed.configureTestingModule({
      imports: [AdminUsersPageComponent],
      providers: [
        { provide: AdminFacade, useValue: adminFacadeMock as unknown as AdminFacade },
        { provide: AuthService, useValue: authServiceMock as unknown as AuthService },
      ],
    });

    const fixture = TestBed.createComponent(AdminUsersPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('E-mail');
    expect(html.textContent).toContain('Papel');
    expect(html.textContent).toContain('Ações');
  });

  it('RF6 T6 / CA4: should hide or disable reset action for logged admin own row', async () => {
    TestBed.configureTestingModule({
      imports: [AdminUsersPageComponent],
      providers: [
        { provide: AdminFacade, useValue: adminFacadeMock as unknown as AdminFacade },
        { provide: AuthService, useValue: authServiceMock as unknown as AuthService },
      ],
    });

    const fixture = TestBed.createComponent(AdminUsersPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const html = fixture.nativeElement as HTMLElement;
    const ownRowResetButton = html.querySelector('[data-testid="admin-user-reset-self"]');
    expect(ownRowResetButton).toBeTruthy();
    const isDisabled = ownRowResetButton?.hasAttribute('disabled') ?? false;
    expect(isDisabled).toBe(true);
  });
});

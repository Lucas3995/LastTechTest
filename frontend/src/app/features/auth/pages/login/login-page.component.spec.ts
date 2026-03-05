import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';

import { LoginPageComponent } from './login-page.component';
import { AuthService, AuthSession } from '../../../../core';

describe('LoginPageComponent', () => {
  let component: LoginPageComponent;
  let fixture: ComponentFixture<LoginPageComponent>;

  const authServiceMock = {
    login: vi.fn(),
    setSession: vi.fn(),
  } as Partial<AuthService> as AuthService;

  const routerMock = {
    navigateByUrl: vi.fn(),
  } as Partial<Router> as Router;

  const queryParamMapGet = vi.fn((key: string) =>
    key === 'returnUrl' ? '/anticipation/my-requests' : null,
  );
  const activatedRouteMock = {
    snapshot: {
      queryParamMap: { get: queryParamMapGet },
    },
  } as unknown as ActivatedRoute;

  beforeEach(async () => {
    queryParamMapGet.mockImplementation((key: string) =>
      key === 'returnUrl' ? '/anticipation/my-requests' : null,
    );
    await TestBed.configureTestingModule({
      imports: [LoginPageComponent],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('deve bloquear submit com formulario invalido', () => {
    component.form.setValue({ email: '', password: '' });
    component.onSubmit();

    expect(authServiceMock.login).not.toHaveBeenCalled();
  });

  it('deve realizar login com credenciais validas e redirecionar', () => {
    const session: AuthSession = {
      accessToken: 'token',
      refreshToken: null,
      user: {
        id: 'user-1',
        role: 'Creator',
        creatorId: 'creator-1',
      },
    };

    (authServiceMock.login as any).mockReturnValue(of(session));

    component.form.setValue({
      email: 'creator@example.com',
      password: 'password',
    });

    component.onSubmit();

    expect(authServiceMock.login).toHaveBeenCalled();
    expect(authServiceMock.setSession).toHaveBeenCalledWith(session);
    expect(routerMock.navigateByUrl).toHaveBeenCalledWith('/anticipation/my-requests');
  });

  it('deve redirecionar para /home quando não há returnUrl', () => {
    queryParamMapGet.mockImplementation(() => null);
    expect(component.returnUrl).toBe('/home');
    const session: AuthSession = {
      accessToken: 'token',
      refreshToken: null,
      user: { id: 'u', role: 'Creator', creatorId: 'c' },
    };
    (authServiceMock.login as any).mockReturnValue(of(session));
    component.form.setValue({ email: 'a@b.com', password: 'pwd' });
    component.onSubmit();
    expect(routerMock.navigateByUrl).toHaveBeenCalledWith('/home');
  });

  it('deve exibir mensagem de erro quando login falhar', () => {
    (authServiceMock.login as any).mockReturnValue(
      throwError(() => new Error('Falha de login')),
    );

    component.form.setValue({
      email: 'creator@example.com',
      password: 'password',
    });

    component.onSubmit();

    expect(component.errorMessage).toContain('Nao foi possivel realizar seu login agora.');
  });
});


import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';

import { AuthService, AuthSession } from '../../../../core';
import { buildErrorPresentation } from '../../../../application/anticipation/backend-error.util';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
})
export class LoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  submitting = false;
  errorMessage: string | null = null;
  errorSupportId: string | null = null;

  get returnUrl(): string {
    return this.route.snapshot.queryParamMap.get('returnUrl') ?? '/home';
  }

  onSubmit(): void {
    if (this.form.invalid || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.errorMessage = null;
    this.errorSupportId = null;

    const credentials = this.form.getRawValue();

    this.authService.login(credentials).subscribe({
      next: (session: AuthSession) => {
        this.authService.setSession(session);
        this.submitting = false;
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err: unknown) => {
        const presentation = buildErrorPresentation(
          err,
          'Nao foi possivel realizar seu login agora.',
        );
        this.errorMessage = presentation.contextMessage + (presentation.detailMessage ? ` ${presentation.detailMessage}` : '');
        this.errorSupportId = presentation.supportId ?? null;
        this.submitting = false;
      },
    });
  }
}


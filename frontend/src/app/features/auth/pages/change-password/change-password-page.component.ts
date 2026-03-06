import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { ChangePasswordFacade } from '../../../../application';
import { AuthService } from '../../../../core';

function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
  const newPassword = control.get('newPassword')?.value;
  const confirmNewPassword = control.get('confirmNewPassword')?.value;

  if (!newPassword || !confirmNewPassword) {
    return null;
  }

  return newPassword === confirmNewPassword ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-change-password-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './change-password-page.component.html',
  styleUrl: './change-password-page.component.scss',
})
export class ChangePasswordPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly facade = inject(ChangePasswordFacade);
  private readonly authService = inject(AuthService, { optional: true });
  private readonly router = inject(Router, { optional: true });

  readonly form = this.fb.nonNullable.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required]],
      confirmNewPassword: ['', [Validators.required]],
    },
    {
      validators: [passwordsMatchValidator],
    },
  );

  loading = this.facade.loading.bind(this.facade);
  successMessage = this.facade.successMessage.bind(this.facade);
  errorMessage = this.facade.errorMessage.bind(this.facade);
  errorSupportId = this.facade.errorSupportId.bind(this.facade);

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    await this.facade.submit({
      currentPassword: value.currentPassword,
      newPassword: value.newPassword,
      confirmNewPassword: value.confirmNewPassword,
    });

    if (this.successMessage()) {
      this.authService?.logout();
      await this.router?.navigateByUrl('/?returnUrl=%2Fauth%2Fchange-password');
    }
  }

  async onCancel(): Promise<void> {
    this.facade.clearMessages();
    await this.router?.navigateByUrl('/home');
  }
}

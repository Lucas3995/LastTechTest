import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { CreateAdminUserPayload } from '../../../../domain';

@Component({
  selector: 'app-admin-user-form-drawer',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './admin-user-form-drawer.component.html',
  styleUrl: './admin-user-form-drawer.component.scss',
})
export class AdminUserFormDrawerComponent {
  private readonly fb = inject(FormBuilder);

  @Input() submitting = false;
  @Input() backendErrorMessage: string | null = null;

  @Output() save = new EventEmitter<CreateAdminUserPayload>();
  @Output() drawerClose = new EventEmitter<void>();

  readonly roleOptions = ['Creator', 'Analista', 'Admin'];

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    role: ['Creator', [Validators.required]],
  });

  onSubmit(): void {
    if (this.form.invalid || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    this.save.emit({
      email: raw.email.trim(),
      roles: [raw.role],
    });
  }

  onClose(): void {
    this.drawerClose.emit();
  }
}

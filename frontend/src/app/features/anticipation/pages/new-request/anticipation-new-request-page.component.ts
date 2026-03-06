import { Component, inject, DestroyRef, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AnticipationMyRequestsFacade } from '../../../../application';

@Component({
  selector: 'app-anticipation-new-request-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './anticipation-new-request-page.component.html',
  styleUrl: './anticipation-new-request-page.component.scss',
})
export class AnticipationNewRequestPageComponent {
  private readonly facade = inject(AnticipationMyRequestsFacade);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly errorMessage = this.facade.errorMessage;
  readonly errorSupportId = this.facade.errorSupportId;
  readonly infoMessage = this.facade.infoMessage;

  readonly form = new FormGroup({
    requestedAmount: new FormControl<number | null>(null, {
      validators: [Validators.required, Validators.min(100)],
      nonNullable: false,
    }),
  });

  submitting = false;

  constructor() {
    effect(() => {
      const msg = this.facade.infoMessage();
      if (msg === 'Solicitação criada com sucesso.') {
        this.router.navigate(['anticipation', 'my-requests']);
      }
    });
  }

  get requestedAmountControl(): FormControl<number | null> {
    return this.form.get('requestedAmount') as FormControl<number | null>;
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.submitting) return;
    const value = this.requestedAmountControl.value;
    if (value == null) return;
    this.submitting = true;
    try {
      await this.facade.createRequest({ requestedAmount: value });
    } finally {
      this.submitting = false;
    }
  }

  onCancel(): void {
    this.router.navigate(['anticipation', 'my-requests']);
  }
}

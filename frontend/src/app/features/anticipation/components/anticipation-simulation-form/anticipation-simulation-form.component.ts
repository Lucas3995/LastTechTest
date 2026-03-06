import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SimulateAnticipationPayload } from '../../../../domain';

@Component({
  selector: 'app-anticipation-simulation-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './anticipation-simulation-form.component.html',
  styleUrl: './anticipation-simulation-form.component.scss'
})
export class AnticipationSimulationFormComponent implements OnInit {

  @Input() isSubmitting = false;
  @Input() userRole = 'Creator';
  @Input() errorMessage?: string | null;
  @Input() creatorIdForSimulation?: string;

  @Output() simulationRequested = new EventEmitter<SimulateAnticipationPayload>();
  @Output() creatorSelected = new EventEmitter<string>();

  private fb = inject(FormBuilder);

  form = this.fb.group({
    requestedAmount: ['', [Validators.required, Validators.min(100), Validators.pattern(/^[0-9]+$/)]], // apenas números, mín 100
    creatorId: [''] // para Admin/Analista
  }) as FormGroup;

  ngOnInit(): void {
    // Se creatorId fornecido, pré-popular
    if (this.creatorIdForSimulation) {
      this.form.patchValue({ creatorId: this.creatorIdForSimulation });
    }
  }

  onSubmit(): void {
    if (this.form.valid && !this.isSubmitting) {
      const payload: SimulateAnticipationPayload = {
        requestedAmount: this.form.value.requestedAmount,
        creatorId: this.form.value.creatorId || undefined
      };
      this.simulationRequested.emit(payload);
    }
  }

  onCreatorSelected(creatorId: string): void {
    this.creatorSelected.emit(creatorId);
  }

  onReset(): void {
    this.form.reset();
  }

  get requestedAmount() {
    return this.form.get('requestedAmount');
  }

  get creatorId() {
    return this.form.get('creatorId');
  }
}


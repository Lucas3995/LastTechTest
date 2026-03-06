import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { AnticipationSimulation } from '../../../../domain';

@Component({
  selector: 'app-confirm-conversion-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  templateUrl: './confirm-conversion-dialog.component.html',
  styleUrl: './confirm-conversion-dialog.component.scss'
})
export class ConfirmConversionDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<ConfirmConversionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AnticipationSimulation
  ) {}

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  formatCurrency(cents: number): string {
    return (cents / 100).toLocaleString('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    });
  }
}
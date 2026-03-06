import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

export interface AdminResetDialogData {
  email: string;
}

@Component({
  selector: 'app-admin-reset-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  templateUrl: './admin-reset-dialog.component.html',
  styleUrl: './admin-reset-dialog.component.scss',
})
export class AdminResetDialogComponent {
  readonly dialogRef = inject(MatDialogRef<AdminResetDialogComponent>, { optional: true });
  readonly data = inject<AdminResetDialogData | null>(MAT_DIALOG_DATA, { optional: true }) ?? {
    email: '',
  };

  onCancel(): void {
    this.dialogRef?.close(false);
  }

  onConfirm(): void {
    this.dialogRef?.close(true);
  }
}

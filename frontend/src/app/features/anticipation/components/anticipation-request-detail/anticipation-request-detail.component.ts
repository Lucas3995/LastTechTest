import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AnticipationRequest, AnticipationRequestStatus } from '../../../../domain';
import { AnticipationStatusLabelPipe } from '../../../../shared/pipes/anticipation-status-label.pipe';

@Component({
  selector: 'app-anticipation-request-detail',
  standalone: true,
  imports: [CommonModule, AnticipationStatusLabelPipe],
  templateUrl: './anticipation-request-detail.component.html',
  styleUrl: './anticipation-request-detail.component.scss',
})
export class AnticipationRequestDetailComponent {
  @Input() request: AnticipationRequest | null = null;
  /** When true (e.g. Admin context), show creator row. */
  @Input() showCreator = false;
  /** When true (e.g. Admin context), hide Cancel button. */
  @Input() hideCancel = false;
  /** When true and request status is Pending, show Aprovar/Recusar buttons. */
  @Input() canShowApproveReject = false;
  @Output() requestCancel = new EventEmitter<string>();
  @Output() requestApprove = new EventEmitter<{ id: string; observation?: string }>();
  @Output() requestReject = new EventEmitter<{ id: string; reason: string }>();

  readonly Pending = AnticipationRequestStatus.Pending;

  showConfirmDialog = false;
  showRejectDialog = false;
  rejectReason = '';
  rejectValidationMessage = '';

  onRequestCancel(): void {
    if (!this.request) {
      return;
    }
    this.showConfirmDialog = true;
  }

  onConfirmCancel(): void {
    if (!this.request) {
      return;
    }
    this.showConfirmDialog = false;
    this.requestCancel.emit(this.request.id);
  }

  onCloseDialog(): void {
    this.showConfirmDialog = false;
  }

  onRequestApprove(): void {
    if (!this.request) return;
    this.requestApprove.emit({ id: this.request.id });
  }

  onRequestReject(): void {
    if (!this.request) return;
    this.rejectReason = '';
    this.rejectValidationMessage = '';
    this.showRejectDialog = true;
  }

  onConfirmReject(): void {
    const reason = this.rejectReason?.trim() ?? '';
    if (!reason) {
      this.rejectValidationMessage = 'Informe o motivo da recusa.';
      return;
    }
    if (!this.request) return;
    this.showRejectDialog = false;
    this.rejectReason = '';
    this.rejectValidationMessage = '';
    this.requestReject.emit({ id: this.request.id, reason });
  }

  onCloseRejectDialog(): void {
    this.showRejectDialog = false;
    this.rejectReason = '';
    this.rejectValidationMessage = '';
  }
}


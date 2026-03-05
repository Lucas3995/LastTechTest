import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AnticipationRequest } from '../../../../domain';

@Component({
  selector: 'app-anticipation-request-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './anticipation-request-detail.component.html',
  styleUrl: './anticipation-request-detail.component.scss',
})
export class AnticipationRequestDetailComponent {
  @Input() request: AnticipationRequest | null = null;
  @Output() requestCancel = new EventEmitter<string>();

  showConfirmDialog = false;

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
}


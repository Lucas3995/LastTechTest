import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AnticipationRequest } from '../../../../domain';

@Component({
  selector: 'app-anticipation-requests-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './anticipation-requests-table.component.html',
  styleUrl: './anticipation-requests-table.component.scss',
})
export class AnticipationRequestsTableComponent {
  @Input() requests: AnticipationRequest[] = [];
  @Output() requestSelected = new EventEmitter<string>();

  sortDirection: 'none' | 'ascending' | 'descending' = 'none';

  onRowClick(request: AnticipationRequest): void {
    this.requestSelected.emit(request.id);
  }

  onToggleAmountSort(): void {
    this.sortDirection =
      this.sortDirection === 'ascending' || this.sortDirection === 'none'
        ? 'descending'
        : 'ascending';
  }
}


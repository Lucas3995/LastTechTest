import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AnticipationRequest } from '../../../../domain';
import { AnticipationStatusLabelPipe } from '../../../../shared/pipes/anticipation-status-label.pipe';

@Component({
  selector: 'app-anticipation-admin-requests-table',
  standalone: true,
  imports: [CommonModule, AnticipationStatusLabelPipe],
  templateUrl: './anticipation-admin-requests-table.component.html',
  styleUrl: './anticipation-admin-requests-table.component.scss',
})
export class AnticipationAdminRequestsTableComponent {
  @Input() requests: AnticipationRequest[] = [];
  @Input() totalCount = 0;
  @Input() currentPage = 1;
  @Input() pageSize = 20;
  @Output() requestSelected = new EventEmitter<AnticipationRequest>();
  @Output() pageChange = new EventEmitter<number>();

  sortDirection: 'none' | 'ascending' | 'descending' = 'none';

  get totalPages(): number {
    if (this.pageSize <= 0) return 0;
    return Math.ceil(this.totalCount / this.pageSize) || 1;
  }

  onRowClick(request: AnticipationRequest): void {
    this.requestSelected.emit(request);
  }

  onToggleSort(): void {
    this.sortDirection =
      this.sortDirection === 'ascending' || this.sortDirection === 'none'
        ? 'descending'
        : 'ascending';
  }

  goToPreviousPage(): void {
    if (this.currentPage > 1) {
      this.pageChange.emit(this.currentPage - 1);
    }
  }

  goToNextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.pageChange.emit(this.currentPage + 1);
    }
  }
}

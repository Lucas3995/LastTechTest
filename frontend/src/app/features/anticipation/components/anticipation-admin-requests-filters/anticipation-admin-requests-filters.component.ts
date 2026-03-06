import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  AnticipationAdminListFilter,
  AnticipationRequestStatus,
} from '../../../../domain';

@Component({
  selector: 'app-anticipation-admin-requests-filters',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './anticipation-admin-requests-filters.component.html',
  styleUrl: './anticipation-admin-requests-filters.component.scss',
})
export class AnticipationAdminRequestsFiltersComponent {
  @Input() activeFilter: AnticipationAdminListFilter | undefined;
  @Output() filterApply = new EventEmitter<AnticipationAdminListFilter>();
  @Output() filterClear = new EventEmitter<void>();

  creatorId = '';
  selectedStatus = '';
  readonly pageSize = 20;

  readonly statusOptions: { value: string; label: string }[] = [
    { value: '', label: 'Todos' },
    { value: AnticipationRequestStatus.Pending, label: 'Em analise' },
    { value: AnticipationRequestStatus.Approved, label: 'Aprovada' },
    { value: AnticipationRequestStatus.Rejected, label: 'Recusada' },
    { value: AnticipationRequestStatus.CanceledByCreator, label: 'Cancelada pelo creator' },
  ];

  get hasActiveFilters(): boolean {
    const f = this.activeFilter;
    return !!(f?.creatorId || (f?.statuses && f.statuses.length > 0) || f?.period);
  }

  onApply(): void {
    const filter: AnticipationAdminListFilter = {
      page: 1,
      pageSize: this.pageSize,
      creatorId: this.creatorId.trim() || undefined,
      statuses:
        this.selectedStatus ?
          [this.selectedStatus as AnticipationRequestStatus]
        : undefined,
      period: this.activeFilter?.period,
    };
    this.filterApply.emit(filter);
  }

  onClear(): void {
    this.creatorId = '';
    this.selectedStatus = '';
    this.filterClear.emit();
  }
}

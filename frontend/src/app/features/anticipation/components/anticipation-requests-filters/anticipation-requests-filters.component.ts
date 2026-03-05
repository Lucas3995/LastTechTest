import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AnticipationRequestStatus, AnticipationRequestsFilter } from '../../../../domain';

@Component({
  selector: 'app-anticipation-requests-filters',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './anticipation-requests-filters.component.html',
  styleUrl: './anticipation-requests-filters.component.scss',
})
export class AnticipationRequestsFiltersComponent {
  private _filters: AnticipationRequestsFilter | undefined;

  @Input() set filters(value: AnticipationRequestsFilter | undefined) {
    this._filters = value;
    this.selectedStatus = value?.statuses?.[0] ?? '';
  }
  @Output() apply = new EventEmitter<AnticipationRequestsFilter>();

  /** Valor do select de status (string do enum ou '' para Todos). */
  selectedStatus = '';

  readonly statusOptions: { value: string; label: string }[] = [
    { value: '', label: 'Todos' },
    { value: AnticipationRequestStatus.Pending, label: 'Em analise' },
    { value: AnticipationRequestStatus.Approved, label: 'Aprovada' },
    { value: AnticipationRequestStatus.Rejected, label: 'Recusada' },
    { value: AnticipationRequestStatus.CanceledByCreator, label: 'Cancelada pelo creator' },
  ];

  onApply(selectEl: HTMLSelectElement): void {
    const status = selectEl.value;
    this.selectedStatus = status;
    const filter: AnticipationRequestsFilter = {
      period: this._filters?.period,
      statuses: status ? [status as AnticipationRequestStatus] : undefined,
    };
    this.apply.emit(filter);
  }
}


import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AnticipationRequestsFilter } from '../../../../domain';

@Component({
  selector: 'app-anticipation-requests-filters',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './anticipation-requests-filters.component.html',
  styleUrl: './anticipation-requests-filters.component.scss',
})
export class AnticipationRequestsFiltersComponent {
  @Input() filters: AnticipationRequestsFilter | undefined;
  @Output() apply = new EventEmitter<void>();

  onApply(): void {
    this.apply.emit();
  }
}


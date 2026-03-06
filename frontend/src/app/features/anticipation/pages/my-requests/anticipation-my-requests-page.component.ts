import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AnticipationMyRequestsFacade } from '../../../../application';
import { AnticipationRequestsFilter } from '../../../../domain';
import { AnticipationRequestsFiltersComponent } from '../../components/anticipation-requests-filters/anticipation-requests-filters.component';
import { AnticipationRequestsTableComponent } from '../../components/anticipation-requests-table/anticipation-requests-table.component';
import { AnticipationRequestEmptyStateComponent } from '../../components/anticipation-request-empty-state/anticipation-request-empty-state.component';
import { AnticipationRequestDetailComponent } from '../../components/anticipation-request-detail/anticipation-request-detail.component';

@Component({
  selector: 'app-anticipation-my-requests-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    AnticipationRequestsFiltersComponent,
    AnticipationRequestsTableComponent,
    AnticipationRequestEmptyStateComponent,
    AnticipationRequestDetailComponent,
  ],
  templateUrl: './anticipation-my-requests-page.component.html',
  styleUrl: './anticipation-my-requests-page.component.scss',
})
export class AnticipationMyRequestsPageComponent implements OnInit {
  private readonly facade = inject(AnticipationMyRequestsFacade);

  readonly loading = this.facade.loading;
  readonly errorMessage = this.facade.errorMessage;
  readonly infoMessage = this.facade.infoMessage;
  readonly errorSupportId = this.facade.errorSupportId;
  readonly filters = this.facade.filters;
  readonly requests = this.facade.requests;
  readonly selectedRequest = this.facade.selectedRequest;

  ngOnInit(): void {
    this.facade.loadInitialRequests();
  }

  onApplyFilters(emittedFilter: AnticipationRequestsFilter): void {
    this.facade.applyFilters(emittedFilter);
  }

  onRequestSelected(id: string): void {
    this.facade.selectRequest(id);
  }

  onCancelRequest(id: string): void {
    this.facade.cancelRequest(id);
  }
}


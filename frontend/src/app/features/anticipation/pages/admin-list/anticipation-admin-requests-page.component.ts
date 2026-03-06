import { ChangeDetectorRef, Component, OnInit, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnticipationAdminListFacade } from '../../../../application';
import { AnticipationAdminListFilter, AnticipationRequestStatus } from '../../../../domain';
import { AnticipationAdminRequestsFiltersComponent } from '../../components/anticipation-admin-requests-filters/anticipation-admin-requests-filters.component';
import { AnticipationAdminRequestsTableComponent } from '../../components/anticipation-admin-requests-table/anticipation-admin-requests-table.component';
import { AnticipationRequestDetailComponent } from '../../components/anticipation-request-detail/anticipation-request-detail.component';

@Component({
  selector: 'app-anticipation-admin-requests-page',
  standalone: true,
  imports: [
    CommonModule,
    AnticipationAdminRequestsFiltersComponent,
    AnticipationAdminRequestsTableComponent,
    AnticipationRequestDetailComponent,
  ],
  templateUrl: './anticipation-admin-requests-page.component.html',
  styleUrl: './anticipation-admin-requests-page.component.scss',
})
export class AnticipationAdminRequestsPageComponent implements OnInit {
  private readonly facade = inject(AnticipationAdminListFacade);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly loading = this.facade.loading;
  readonly errorMessage = this.facade.errorMessage;
  readonly errorSupportId = this.facade.errorSupportId;
  readonly infoMessage = this.facade.infoMessage;
  readonly filters = this.facade.filters;
  readonly requests = this.facade.requests;
  readonly totalCount = this.facade.totalCount;
  readonly selectedRequest = this.facade.selectedRequest;

  readonly canShowApproveReject = computed(
    () => this.facade.selectedRequest()?.status === AnticipationRequestStatus.Pending,
  );

  async ngOnInit(): Promise<void> {
    await this.facade.loadInitial();
    this.cdr.detectChanges();
  }

  onApplyFilters(filter: AnticipationAdminListFilter): void {
    this.facade.applyFilters(filter);
  }

  onClearFilters(): void {
    this.facade.clearFilters();
  }

  onRequestSelected(request: { id: string }): void {
    this.facade.selectRequest(request.id);
  }

  onSetPage(page: number): void {
    this.facade.setPage(page);
  }

  onRequestApprove(payload: { id: string; observation?: string }): void {
    this.facade.approveRequest(payload.id, payload.observation);
  }

  onRequestReject(payload: { id: string; reason: string }): void {
    this.facade.rejectRequest(payload.id, payload.reason);
  }
}

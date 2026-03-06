import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import {
  AnticipationAdminListFilter,
  AnticipationRequest,
  AnticipationRequestStatus,
  AnticipationRequestsFilter,
  AnticipationRequestsPort,
  CreateAnticipationRequestPayload,
  CreateAnticipationRequestResult,
} from '../../domain';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_BASE_URL } from '../../core/api-base-url';

/** Backend list item: Id, Protocol, CreatorId, Status (string), RequestedAmount, NetAmount, RequestedAtUtc, CreatedAtUtc */
interface AnticipationRequestListItemBackend {
  id: string;
  protocol: string;
  creatorId: string;
  status: string;
  requestedAmount: number;
  netAmount: number;
  requestedAtUtc: string;
  createdAtUtc: string;
}

interface ListMyRequestsResponse {
  items: AnticipationRequestListItemBackend[];
  totalCount: number;
}

/** Backend GetById response */
interface GetAnticipationRequestByIdResponseBackend {
  id: string;
  protocol: string;
  creatorId: string;
  status: string;
  requestedAmount: number;
  grossAmount: number;
  feesAmount: number;
  netAmount: number;
  requestedAtUtc: string;
  createdAtUtc: string;
}

/** Backend Cancel response: Id, Protocol, Status, AlreadyCanceled */
interface CancelAnticipationRequestResponseBackend {
  id: string;
  protocol: string;
  status: string;
  alreadyCanceled: boolean;
}

/** Backend Approve/Reject response: Id, Protocol, Status */
interface ApproveRejectResponseBackend {
  id: string;
  protocol: string;
  status: string;
}

/** Backend Create (POST) response: Id, Protocol, NetAmount, Status (RF-5) */
interface CreateAnticipationRequestResponseBackend {
  id: string;
  protocol: string;
  netAmount: number;
  status: string;
}

@Injectable()
export class AnticipationRequestsHttpService implements AnticipationRequestsPort {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);
  private readonly baseUrl = `${this.apiBaseUrl}/api/v1/anticipations`;

  listGlobalRequests(filter: AnticipationAdminListFilter): Observable<{ items: AnticipationRequest[]; totalCount: number }> {
    let params = new HttpParams()
      .set('page', String(filter.page))
      .set('pageSize', String(filter.pageSize));

    if (filter.creatorId) {
      params = params.set('creatorId', filter.creatorId);
    }
    if (filter.period) {
      params = params
        .set('fromUtc', filter.period.from.toISOString())
        .set('toUtc', filter.period.to.toISOString());
    }
    if (filter.statuses && filter.statuses.length > 0) {
      const statusInt = this.mapStatusToBackend(filter.statuses[0]);
      params = params.set('status', String(statusInt));
    }

    return this.http.get<ListMyRequestsResponse>(`${this.baseUrl}`, { params }).pipe(
      map((response) => ({
        items: (response.items ?? []).map((item) => this.mapListItemToDomain(item)),
        totalCount: response.totalCount ?? 0,
      })),
    );
  }

  listMyRequests(filter?: AnticipationRequestsFilter): Observable<AnticipationRequest[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', '100');

    if (filter?.period) {
      params = params
        .set('fromUtc', filter.period.from.toISOString())
        .set('toUtc', filter.period.to.toISOString());
    }

    if (filter?.statuses && filter.statuses.length > 0) {
      const statusInt = this.mapStatusToBackend(filter.statuses[0]);
      params = params.set('status', String(statusInt));
    }

    return this.http.get<ListMyRequestsResponse>(`${this.baseUrl}`, { params }).pipe(
      map((response) =>
        (response.items ?? []).map((item) => this.mapListItemToDomain(item)),
      ),
    );
  }

  getRequestDetail(id: string): Observable<AnticipationRequest> {
    return this.http
      .get<GetAnticipationRequestByIdResponseBackend>(
        `${this.baseUrl}/${encodeURIComponent(id)}`,
      )
      .pipe(map((d) => this.mapDetailToDomain(d)));
  }

  cancelRequest(id: string): Observable<AnticipationRequest> {
    return this.http
      .post<CancelAnticipationRequestResponseBackend>(
        `${this.baseUrl}/${encodeURIComponent(id)}/cancel`,
        {},
      )
      .pipe(map((d) => this.mapCancelResponseToDomain(d)));
  }

  approveRequest(id: string, observation?: string): Observable<AnticipationRequest> {
    return this.http
      .post<ApproveRejectResponseBackend>(
        `${this.baseUrl}/${encodeURIComponent(id)}/approve`,
        { observation: observation ?? '' },
      )
      .pipe(map((d) => this.mapApproveRejectResponseToDomain(d)));
  }

  rejectRequest(id: string, reason: string): Observable<AnticipationRequest> {
    return this.http
      .post<ApproveRejectResponseBackend>(
        `${this.baseUrl}/${encodeURIComponent(id)}/reject`,
        { reason },
      )
      .pipe(map((d) => this.mapApproveRejectResponseToDomain(d)));
  }

  createRequest(payload: CreateAnticipationRequestPayload): Observable<CreateAnticipationRequestResult> {
    const body: { requestedAmount: number; creatorId?: string } = {
      requestedAmount: payload.requestedAmount,
    };
    if (payload.creatorId != null) {
      body.creatorId = payload.creatorId;
    }
    return this.http
      .post<CreateAnticipationRequestResponseBackend>(this.baseUrl, body)
      .pipe(map((d) => this.mapCreateResponseToDomain(d)));
  }

  private mapCreateResponseToDomain(
    d: CreateAnticipationRequestResponseBackend,
  ): CreateAnticipationRequestResult {
    return {
      id: d.id,
      protocol: d.protocol,
      netAmount: d.netAmount ?? 0,
      status: this.mapBackendStatusToDomain(d.status),
    };
  }

  private mapListItemToDomain(
    item: AnticipationRequestListItemBackend,
  ): AnticipationRequest {
    return {
      id: item.id,
      creatorId: item.creatorId,
      createdAt: item.createdAtUtc,
      grossAmountCents: Math.round((item.requestedAmount ?? 0) * 100),
      netAmountCents: Math.round((item.netAmount ?? 0) * 100),
      status: this.mapBackendStatusToDomain(item.status),
    };
  }

  private mapDetailToDomain(
    d: GetAnticipationRequestByIdResponseBackend,
  ): AnticipationRequest {
    return {
      id: d.id,
      creatorId: d.creatorId,
      createdAt: d.createdAtUtc,
      grossAmountCents: Math.round((d.grossAmount ?? 0) * 100),
      netAmountCents: Math.round((d.netAmount ?? 0) * 100),
      status: this.mapBackendStatusToDomain(d.status),
    };
  }

  private mapCancelResponseToDomain(
    d: CancelAnticipationRequestResponseBackend,
  ): AnticipationRequest {
    return {
      id: d.id,
      creatorId: '',
      createdAt: '',
      grossAmountCents: 0,
      netAmountCents: 0,
      status: this.mapBackendStatusToDomain(d.status),
    };
  }

  private mapApproveRejectResponseToDomain(
    d: ApproveRejectResponseBackend,
  ): AnticipationRequest {
    return {
      id: d.id,
      creatorId: '',
      createdAt: '',
      grossAmountCents: 0,
      netAmountCents: 0,
      status: this.mapBackendStatusToDomain(d.status),
    };
  }

  /** Backend enum: Created=0, Pending=1, Approved=2, Rejected=3, CanceledByCreator=4 */
  private mapStatusToBackend(status: AnticipationRequestStatus): number {
    switch (status) {
      case AnticipationRequestStatus.Pending:
        return 1;
      case AnticipationRequestStatus.Approved:
        return 2;
      case AnticipationRequestStatus.Rejected:
        return 3;
      case AnticipationRequestStatus.CanceledByCreator:
        return 4;
      default:
        return 1;
    }
  }

  private mapBackendStatusToDomain(status: string): AnticipationRequestStatus {
    switch (status) {
      case 'Created':
        return AnticipationRequestStatus.Pending;
      case 'Pending':
        return AnticipationRequestStatus.Pending;
      case 'Approved':
        return AnticipationRequestStatus.Approved;
      case 'Rejected':
        return AnticipationRequestStatus.Rejected;
      case 'CanceledByCreator':
        return AnticipationRequestStatus.CanceledByCreator;
      default:
        return AnticipationRequestStatus.Pending;
    }
  }
}


import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import {
  AnticipationAdminListFilter,
  AnticipationRequest,
  AnticipationRequestsFilter,
  AnticipationRequestsPort,
  CreateAnticipationRequestPayload,
  CreateAnticipationRequestResult,
  SimulateAnticipationPayload,
  SimulationResult,
  ConversionResult,
} from '../../domain';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_BASE_URL } from '../../core/api-base-url';
import {
  ListMyRequestsResponse,
  GetAnticipationRequestByIdResponseBackend,
  CancelAnticipationRequestResponseBackend,
  ApproveRejectResponseBackend,
  CreateAnticipationRequestResponseBackend,
  SimulateAnticipationResponseBackend,
  ConvertSimulationResponseBackend,
  mapStatusToBackend,
  mapListItemToDomain,
  mapDetailToDomain,
  mapCancelResponseToDomain,
  mapApproveRejectResponseToDomain,
  mapCreateResponseToDomain,
  mapSimulateResponseToDomain,
  mapConvertResponseToDomain,
} from './anticipation-request.mapper';

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
      const statusInt = mapStatusToBackend(filter.statuses[0]);
      params = params.set('status', String(statusInt));
    }

    return this.http.get<ListMyRequestsResponse>(`${this.baseUrl}`, { params }).pipe(
      map((response) => ({
        items: (response.items ?? []).map((item) => mapListItemToDomain(item)),
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
      const statusInt = mapStatusToBackend(filter.statuses[0]);
      params = params.set('status', String(statusInt));
    }

    return this.http.get<ListMyRequestsResponse>(`${this.baseUrl}`, { params }).pipe(
      map((response) =>
        (response.items ?? []).map((item) => mapListItemToDomain(item)),
      ),
    );
  }

  getRequestDetail(id: string): Observable<AnticipationRequest> {
    return this.http
      .get<GetAnticipationRequestByIdResponseBackend>(
        `${this.baseUrl}/${encodeURIComponent(id)}`,
      )
      .pipe(map((d) => mapDetailToDomain(d)));
  }

  cancelRequest(id: string): Observable<AnticipationRequest> {
    return this.http
      .post<CancelAnticipationRequestResponseBackend>(
        `${this.baseUrl}/${encodeURIComponent(id)}/cancel`,
        {},
      )
      .pipe(map((d) => mapCancelResponseToDomain(d)));
  }

  approveRequest(id: string, observation?: string): Observable<AnticipationRequest> {
    return this.http
      .post<ApproveRejectResponseBackend>(
        `${this.baseUrl}/${encodeURIComponent(id)}/approve`,
        { observation: observation ?? '' },
      )
      .pipe(map((d) => mapApproveRejectResponseToDomain(d)));
  }

  rejectRequest(id: string, reason: string): Observable<AnticipationRequest> {
    return this.http
      .post<ApproveRejectResponseBackend>(
        `${this.baseUrl}/${encodeURIComponent(id)}/reject`,
        { reason },
      )
      .pipe(map((d) => mapApproveRejectResponseToDomain(d)));
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
      .pipe(map((d) => mapCreateResponseToDomain(d)));
  }

  simulateAnticipation(payload: SimulateAnticipationPayload): Observable<SimulationResult> {
    const body: { requestedAmount: number; creatorId?: string; contractIds?: string[] } = {
      requestedAmount: payload.requestedAmount,
    };
    if (payload.creatorId != null) {
      body.creatorId = payload.creatorId;
    }
    if (payload.contractIds != null) {
      body.contractIds = payload.contractIds;
    }
    return this.http
      .post<SimulateAnticipationResponseBackend>(`${this.baseUrl}/simulations`, body)
      .pipe(map((response) => mapSimulateResponseToDomain(response)));
  }

  convertSimulationToReal(simulationCode: string, creatorId?: string): Observable<ConversionResult> {
    const body = creatorId ? { creatorId } : {};
    return this.http
      .post<ConvertSimulationResponseBackend>(
        `${this.baseUrl}/simulations/${encodeURIComponent(simulationCode)}/confirm`,
        body
      )
      .pipe(map((response) => mapConvertResponseToDomain(response)));
  }
}


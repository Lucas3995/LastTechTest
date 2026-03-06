import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

export enum AnticipationRequestStatus {
  Pending = 'PENDING',
  Approved = 'APPROVED',
  Rejected = 'REJECTED',
  CanceledByCreator = 'CANCELED_BY_CREATOR',
}

export interface AnticipationRequest {
  id: string;
  creatorId: string;
  createdAt: string; // ISO date string
  grossAmountCents: number;
  netAmountCents: number;
  status: AnticipationRequestStatus;
}

export interface AnticipationRequestsFilterPeriod {
  from: Date;
  to: Date;
}

export interface AnticipationRequestsFilter {
  period?: AnticipationRequestsFilterPeriod;
  statuses?: AnticipationRequestStatus[];
}

/** Filter for admin global list. Used by listGlobalRequests. */
export interface AnticipationAdminListFilter {
  creatorId?: string;
  statuses?: AnticipationRequestStatus[];
  period?: AnticipationRequestsFilterPeriod;
  page: number;
  pageSize: number;
}

export interface AnticipationAdminListResult {
  items: AnticipationRequest[];
  totalCount: number;
}

/** Payload for creating a new anticipation request (RF-5). requestedAmount in reais; creatorId optional for Admin. */
export interface CreateAnticipationRequestPayload {
  requestedAmount: number;
  creatorId?: string;
}

/** Result of creating an anticipation request (RF-5). netAmount in reais. */
export interface CreateAnticipationRequestResult {
  id: string;
  protocol: string;
  netAmount: number;
  status: AnticipationRequestStatus;
}

/** Port for loading/listing/cancelling/approving/rejecting anticipation requests. Implemented in infrastructure. */
export interface AnticipationRequestsPort {
  listMyRequests(filter?: AnticipationRequestsFilter): Observable<AnticipationRequest[]>;
  listGlobalRequests(filter: AnticipationAdminListFilter): Observable<AnticipationAdminListResult>;
  getRequestDetail(id: string): Observable<AnticipationRequest>;
  cancelRequest(id: string): Observable<AnticipationRequest>;
  approveRequest(id: string, observation?: string): Observable<AnticipationRequest>;
  rejectRequest(id: string, reason: string): Observable<AnticipationRequest>;
  createRequest(payload: CreateAnticipationRequestPayload): Observable<CreateAnticipationRequestResult>;
}

export const ANTICIPATION_REQUESTS_PORT = new InjectionToken<AnticipationRequestsPort>(
  'AnticipationRequestsPort',
);


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

/** Port for loading/listing/cancelling anticipation requests. Implemented in infrastructure. */
export interface AnticipationRequestsPort {
  listMyRequests(filter?: AnticipationRequestsFilter): Observable<AnticipationRequest[]>;
  getRequestDetail(id: string): Observable<AnticipationRequest>;
  cancelRequest(id: string): Observable<AnticipationRequest>;
}

export const ANTICIPATION_REQUESTS_PORT = new InjectionToken<AnticipationRequestsPort>(
  'AnticipationRequestsPort',
);


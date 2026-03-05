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


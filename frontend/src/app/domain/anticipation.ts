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

/** Port for loading/listing/cancelling/approving/rejecting/simulating anticipation requests. Implemented in infrastructure. */
export interface AnticipationRequestsPort {
  listMyRequests(filter?: AnticipationRequestsFilter): Observable<AnticipationRequest[]>;
  listGlobalRequests(filter: AnticipationAdminListFilter): Observable<AnticipationAdminListResult>;
  getRequestDetail(id: string): Observable<AnticipationRequest>;
  cancelRequest(id: string): Observable<AnticipationRequest>;
  approveRequest(id: string, observation?: string): Observable<AnticipationRequest>;
  rejectRequest(id: string, reason: string): Observable<AnticipationRequest>;
  createRequest(payload: CreateAnticipationRequestPayload): Observable<CreateAnticipationRequestResult>;
  // RF-3 simulation methods
  simulateAnticipation(payload: SimulateAnticipationPayload): Observable<SimulationResult>;
  convertSimulationToReal(simulationCode: string, creatorId?: string): Observable<ConversionResult>;
}

export const ANTICIPATION_REQUESTS_PORT = new InjectionToken<AnticipationRequestsPort>(
  'AnticipationRequestsPort',
);

// Simulation-related types (RF-3)

/** Interface for simulation result */
export interface AnticipationSimulation {
  simulationCode: string;
  validUntil: Date; // ISO 8601 → converted to Date
  grossAmountCents: number; // gross amount in cents
  feesAmountCents: number; // fee in cents
  netAmountCents: number; // net amount in cents
  createdAt: Date;
  anticipationDate?: Date;
  maturityDate?: Date; // optional maturity/expiration date
  creatorId?: string; // for traceability
}

/** Type alias for simulation result */
export type SimulationResult = AnticipationSimulation;

/** Interface for conversion result (creating real request from simulation) */
export interface ConversionResult {
  id: string;
  protocol: string;
  status: AnticipationRequestStatus; // reusing existing enum
  netAmount: number;
}

/** Payload for simulation request */
export interface SimulateAnticipationPayload {
  requestedAmount: number; // requested amount
  creatorId?: string; // for Admin/Analyst (optional)
  contractIds?: string[]; // if needed (per RA-4)
}

/** Enum for conversion error reasons (RF-3 CA-RF3-5) */
export enum SimulationError {
  SIMULATION_EXPIRED = 'SIMULATION_EXPIRED',
  ALREADY_USED = 'ALREADY_USED',
  PENDING_EXISTS = 'PENDING_EXISTS',
  RULES_VIOLATED = 'RULES_VIOLATED',
  UNKNOWN = 'UNKNOWN',
}

/** Alias for backward compatibility */
export type SimulationConversionError = SimulationError;

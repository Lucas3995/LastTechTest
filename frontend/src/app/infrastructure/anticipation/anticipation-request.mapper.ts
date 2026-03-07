import {
  AnticipationRequest,
  AnticipationRequestStatus,
  CreateAnticipationRequestResult,
  SimulationResult,
  ConversionResult,
} from '../../domain';

/** Backend list item: Id, Protocol, CreatorId, Status (string), RequestedAmount, NetAmount, RequestedAtUtc, CreatedAtUtc */
export interface AnticipationRequestListItemBackend {
  id: string;
  protocol: string;
  creatorId: string;
  status: string;
  requestedAmount: number;
  netAmount: number;
  requestedAtUtc: string;
  createdAtUtc: string;
}

export interface ListMyRequestsResponse {
  items: AnticipationRequestListItemBackend[];
  totalCount: number;
}

/** Backend GetById response */
export interface GetAnticipationRequestByIdResponseBackend {
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
export interface CancelAnticipationRequestResponseBackend {
  id: string;
  protocol: string;
  status: string;
  alreadyCanceled: boolean;
}

/** Backend Approve/Reject response: Id, Protocol, Status */
export interface ApproveRejectResponseBackend {
  id: string;
  protocol: string;
  status: string;
}

/** Backend Create (POST) response: Id, Protocol, NetAmount, Status (RF-5) */
export interface CreateAnticipationRequestResponseBackend {
  id: string;
  protocol: string;
  netAmount: number;
  status: string;
}

/** Backend Simulate response (RF-3) */
export interface SimulateAnticipationResponseBackend {
  simulationCode: string;
  validUntilUtc: string;
  grossAmount: number;
  feesAmount: number;
  netAmount: number;
  requestedAmount: number;
  creatorId?: string;
}

/** Backend Convert simulation response (RF-3) */
export interface ConvertSimulationResponseBackend {
  id: string;
  protocol: string;
  status: string;
  netAmount: number;
  createdAt: string;
}

/** Backend enum: Created=0, Pending=1, Approved=2, Rejected=3, CanceledByCreator=4 */
export function mapStatusToBackend(status: AnticipationRequestStatus): number {
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

export function mapBackendStatusToDomain(status: string): AnticipationRequestStatus {
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

export function mapListItemToDomain(
  item: AnticipationRequestListItemBackend,
): AnticipationRequest {
  return {
    id: item.id,
    creatorId: item.creatorId,
    createdAt: item.createdAtUtc,
    grossAmountCents: Math.round((item.requestedAmount ?? 0) * 100),
    netAmountCents: Math.round((item.netAmount ?? 0) * 100),
    status: mapBackendStatusToDomain(item.status),
  };
}

export function mapDetailToDomain(
  d: GetAnticipationRequestByIdResponseBackend,
): AnticipationRequest {
  return {
    id: d.id,
    creatorId: d.creatorId,
    createdAt: d.createdAtUtc,
    grossAmountCents: Math.round((d.grossAmount ?? 0) * 100),
    netAmountCents: Math.round((d.netAmount ?? 0) * 100),
    status: mapBackendStatusToDomain(d.status),
  };
}

export function mapCancelResponseToDomain(
  d: CancelAnticipationRequestResponseBackend,
): AnticipationRequest {
  return {
    id: d.id,
    creatorId: '',
    createdAt: '',
    grossAmountCents: 0,
    netAmountCents: 0,
    status: mapBackendStatusToDomain(d.status),
  };
}

export function mapApproveRejectResponseToDomain(
  d: ApproveRejectResponseBackend,
): AnticipationRequest {
  return {
    id: d.id,
    creatorId: '',
    createdAt: '',
    grossAmountCents: 0,
    netAmountCents: 0,
    status: mapBackendStatusToDomain(d.status),
  };
}

export function mapCreateResponseToDomain(
  d: CreateAnticipationRequestResponseBackend,
): CreateAnticipationRequestResult {
  return {
    id: d.id,
    protocol: d.protocol,
    netAmount: d.netAmount ?? 0,
    status: mapBackendStatusToDomain(d.status),
  };
}

export function mapSimulateResponseToDomain(
  response: SimulateAnticipationResponseBackend,
): SimulationResult {
  return {
    simulationCode: response.simulationCode,
    validUntil: new Date(response.validUntilUtc),
    grossAmountCents: Math.round(response.grossAmount * 100),
    feesAmountCents: Math.round(response.feesAmount * 100),
    netAmountCents: Math.round(response.netAmount * 100),
    createdAt: new Date(),
    creatorId: response.creatorId,
  };
}

export function mapConvertResponseToDomain(
  response: ConvertSimulationResponseBackend,
): ConversionResult {
  return {
    id: response.id,
    protocol: response.protocol,
    status: mapBackendStatusToDomain(response.status),
    netAmount: response.netAmount,
  };
}

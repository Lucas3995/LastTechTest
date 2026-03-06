import { HttpErrorResponse } from '@angular/common/http';
import { ErrorPresentation } from '../../domain';

export interface BackendErrorPayload {
  code?: string;
  message?: string;
  traceId?: string;
}

export function buildErrorPresentation(
  error: unknown,
  contextMessage: string,
): ErrorPresentation {
  if (error instanceof HttpErrorResponse) {
    const status = error.status;
    const payload = (error.error ?? {}) as BackendErrorPayload | string;

    let detailMessage: string | undefined;
    let code: string | undefined;
    let traceId: string | undefined;

    if (typeof payload === 'string') {
      detailMessage = payload;
    } else if (payload && typeof payload === 'object') {
      detailMessage = payload.message;
      code = payload.code;
      traceId = payload.traceId;
    }

    const supportId =
      traceId ??
      code ??
      (status ? `HTTP_${status}` : undefined);

    return {
      contextMessage,
      detailMessage,
      supportId,
    };
  }

  return {
    contextMessage,
  };
}

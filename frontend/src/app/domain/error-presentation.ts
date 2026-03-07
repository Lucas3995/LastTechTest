import { InjectionToken } from '@angular/core';

/** Presentation of an error for the UI (message, optional detail, optional support id). */
export interface ErrorPresentation {
  contextMessage: string;
  detailMessage?: string;
  supportId?: string;
}

/** Builds an ErrorPresentation from an unknown error and a context message. Provided in app.config (implementation in shared). */
export const ERROR_PRESENTATION_BUILDER = new InjectionToken<
  (error: unknown, contextMessage: string) => ErrorPresentation
>('ERROR_PRESENTATION_BUILDER');

/**
 * Formats an ErrorPresentation into a user-facing message string and extracts the supportId.
 * Used by facades to avoid duplicating the same formatting logic.
 */
export function formatErrorPresentation(presentation: ErrorPresentation): {
  message: string;
  supportId: string | null;
} {
  const parts: string[] = [presentation.contextMessage];

  if (presentation.detailMessage) {
    parts.push(`Motivo: ${presentation.detailMessage}`);
  }

  const supportId = presentation.supportId ?? null;
  if (supportId) {
    parts.push(`Codigo para suporte: ${supportId}`);
  }

  return { message: parts.join(' '), supportId };
}

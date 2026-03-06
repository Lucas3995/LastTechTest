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

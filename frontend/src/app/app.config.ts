import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { routes } from './app.routes';
import { environment } from '../environments/environment';
import { API_BASE_URL } from './core/api-base-url';
import { authTokenInterceptor } from './core/auth/auth-token.interceptor';
import { ANTICIPATION_REQUESTS_PORT, ERROR_PRESENTATION_BUILDER } from './domain';
import { AnticipationRequestsHttpService } from './infrastructure/anticipation/anticipation-requests.http.service';
import { buildErrorPresentation } from './shared/utils/backend-error.util';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([authTokenInterceptor])),
    { provide: API_BASE_URL, useValue: environment.apiUrl },
    { provide: ANTICIPATION_REQUESTS_PORT, useClass: AnticipationRequestsHttpService },
    { provide: ERROR_PRESENTATION_BUILDER, useValue: buildErrorPresentation },
  ],
};

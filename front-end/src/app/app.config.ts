import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors, withXhr } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideToastr } from 'ngx-toastr';
import { routes } from './app.routes';
import { credentialsInterceptor } from './core/interceptors/credentials.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideToastr({
      positionClass: 'toast-bottom-left',
      preventDuplicates: true,
      closeButton: true,
      timeOut: 5000,
      progressBar: true,
      toastClass: 'ngx-toastr',
      tapToDismiss: true,
    }),
    provideHttpClient(withXhr(), withInterceptors([credentialsInterceptor])),
  ],
};

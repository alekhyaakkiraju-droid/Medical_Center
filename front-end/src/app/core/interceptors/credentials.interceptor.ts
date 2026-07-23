import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { CsrfTokenStore } from '../csrf-token.store';

export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  const csrfStore = inject(CsrfTokenStore);
  let headers = req.headers;

  if (['POST', 'PUT', 'DELETE', 'PATCH'].includes(req.method)) {
    const csrfToken = csrfStore.getToken();
    if (csrfToken && !headers.has('X-XSRF-TOKEN')) {
      headers = headers.set('X-XSRF-TOKEN', csrfToken);
    }
  }

  return next(req.clone({ headers, withCredentials: true }));
};

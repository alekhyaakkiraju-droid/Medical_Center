import { TestBed, inject } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthServiceService } from './auth-service.service';
import { environment } from '../../../../environments/environment';
import { EmailConfirmationService } from './email-confirmation.service';

describe('EmailConfirmationService', () => {
  let httpMock: HttpTestingController;
  beforeEach(() => { TestBed.configureTestingModule({ providers: [EmailConfirmationService,         {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({
              headers: { 'Content-Type': 'application/json' },
              withCredentials: true
            })
          }
        }, provideHttpClient(), provideHttpClientTesting()] }); httpMock = TestBed.inject(HttpTestingController); });
  afterEach(() => httpMock.verify());
  it('confirmEmail returns a text response', inject([EmailConfirmationService], (service: EmailConfirmationService) => {
    service.confirmEmail('user-1', 'abc/token').subscribe((result) => { expect(result).toContain('confirmed'); });
    const req = httpMock.expectOne((request) => request.url.startsWith(`${environment.api}/Account/confirm-email`));
    expect(req.request.responseType).toBe('text');
    req.flush('Email confirmed successfully.');
  }));
});

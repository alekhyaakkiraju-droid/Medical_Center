import { TestBed, inject } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthServiceService } from './auth-service.service';
import { environment } from '../../../../environments/environment';
import { ForgotServiceService } from './forgot-service.service';
import { HandleErrorsService } from '../../../shared/service/handle-errors.service';

describe('ForgotServiceService', () => {
  let httpMock: HttpTestingController;
  beforeEach(() => { TestBed.configureTestingModule({ providers: [ForgotServiceService, HandleErrorsService,         {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({
              headers: { 'Content-Type': 'application/json' },
              withCredentials: true
            })
          }
        }, provideHttpClient(), provideHttpClientTesting()] }); httpMock = TestBed.inject(HttpTestingController); });
  afterEach(() => httpMock.verify());
  it('forgetPassword returns typed auth message response', inject([ForgotServiceService], (service: ForgotServiceService) => {
    service.forgetPassword('user@example.com').subscribe((result) => { expect(result.message).toContain('reset link'); });
    httpMock.expectOne(`${environment.api}/Account/forgot-password`).flush({ status: 'Success', message: 'Password reset link sent.' });
  }));
});

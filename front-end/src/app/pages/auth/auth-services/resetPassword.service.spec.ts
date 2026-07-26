import { TestBed, inject } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthServiceService } from './auth-service.service';
import { environment } from '../../../../environments/environment';
import { ResetPasswordService } from './resetPassword.service';
import { HandleErrorsService } from '../../../shared/service/handle-errors.service';

describe('ResetPasswordService', () => {
  let httpMock: HttpTestingController;
  beforeEach(() => { TestBed.configureTestingModule({ providers: [ResetPasswordService, HandleErrorsService,         {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({
              headers: { 'Content-Type': 'application/json' },
              withCredentials: true
            })
          }
        }, provideHttpClient(), provideHttpClientTesting()] }); httpMock = TestBed.inject(HttpTestingController); });
  afterEach(() => httpMock.verify());
  it('resetPassword returns typed auth message response', inject([ResetPasswordService], (service: ResetPasswordService) => {
    service.resetPassword('user@example.com', 'token', 'NewPass123!').subscribe((result) => { expect(result.message).toBe('Password has been reset successfully.'); });
    httpMock.expectOne(`${environment.api}/Account/reset-password`).flush({ message: 'Password has been reset successfully.' });
  }));
});

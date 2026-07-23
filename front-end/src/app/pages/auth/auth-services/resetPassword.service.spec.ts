import { TestBed } from '@angular/core/testing';
import {
  provideHttpClient,
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { ResetPasswordService } from './resetPassword.service';
import { AuthServiceService } from './auth-service.service';
import { HandleErrorsService } from '../../../shared/service/handle-errors.service';
import { environment } from '../../../../environments/environment';

describe('ResetPasswordService', () => {
  let service: ResetPasswordService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ResetPasswordService,
        {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({ withCredentials: true }),
          },
        },
        {
          provide: HandleErrorsService,
          useValue: {
            handleError: (error: unknown) => {
              throw error;
            },
          },
        },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(ResetPasswordService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('resetPassword should post to environment api endpoint', () => {
    service.resetPassword('user@example.com', 'reset-token', 'NewPassword123!').subscribe();

    const req = httpMock.expectOne(`${environment.api}/Account/reset-password`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      email: 'user@example.com',
      token: 'reset-token',
      newPassword: 'NewPassword123!',
    });
    req.flush({ message: 'Password has been reset successfully.' });
  });
});

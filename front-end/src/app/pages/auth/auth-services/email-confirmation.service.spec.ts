import { TestBed } from '@angular/core/testing';
import {
  provideHttpClient,
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { EmailConfirmationService } from './email-confirmation.service';
import { AuthServiceService } from './auth-service.service';
import { environment } from '../../../../environments/environment';

describe('EmailConfirmationService', () => {
  let service: EmailConfirmationService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        EmailConfirmationService,
        {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({ withCredentials: true }),
          },
        },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(EmailConfirmationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('confirmEmail should call environment api endpoint', () => {
    service.confirmEmail('user-1', 'token+value').subscribe();

    const req = httpMock.expectOne((request) =>
      request.url.startsWith(`${environment.api}/Account/confirm-email?userId=user-1&token=`)
    );
    expect(req.request.method).toBe('GET');
    expect(req.request.url).toContain(encodeURIComponent('token+value'));
    req.flush('confirmed');
  });
});

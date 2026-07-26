import { TestBed, inject } from '@angular/core/testing';
import {
  HttpClient,
  provideHttpClient,
  withInterceptors
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { ContactService } from './contact.service';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { credentialsInterceptor } from '../../../core/interceptors/credentials.interceptor';
import { environment } from '../../../../environments/environment';

describe('ContactService', () => {
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ContactService,
        {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({
              headers: { 'Content-Type': 'application/json' },
              withCredentials: true
            })
          }
        },
        provideHttpClient(withInterceptors([credentialsInterceptor])),
        provideHttpClientTesting()
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', inject([ContactService], (service: ContactService) => {
    expect(service).toBeTruthy();
  }));

  it('should POST contact inquiry to /api/Contact', inject(
    [ContactService, HttpClient],
    (service: ContactService) => {
      const inquiry = {
        name: 'Jane Doe',
        email: 'jane@example.com',
        phone: '5551234567',
        message: 'Hello'
      };

      service.submitInquiry(inquiry).subscribe();

      const req = httpMock.expectOne(`${environment.api}/Contact`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(inquiry);
      expect(req.request.withCredentials).toBeTrue();
      req.flush({});
    }
  ));
});

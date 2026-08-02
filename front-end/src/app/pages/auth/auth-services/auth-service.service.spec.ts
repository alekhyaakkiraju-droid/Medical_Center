import { TestBed } from '@angular/core/testing';
import {
  HttpClient,
  provideHttpClient,
  withInterceptors,
  withXhr
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { ToastrService } from 'ngx-toastr';
import { AuthServiceService } from './auth-service.service';
import { CsrfTokenStore } from '../../../core/csrf-token.store';
import { credentialsInterceptor } from '../../../core/interceptors/credentials.interceptor';
import { environment } from '../../../../environments/environment';

describe('AuthServiceService', () => {
  let service: AuthServiceService;
  let httpMock: HttpTestingController;
  let csrfStore: CsrfTokenStore;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuthServiceService,
        CsrfTokenStore,
        provideHttpClient(withXhr(), withInterceptors([credentialsInterceptor])),
        provideHttpClientTesting(),
        {
          provide: ToastrService,
          useValue: {
            info: jasmine.createSpy('info'),
          },
        },
      ],
    });

    service = TestBed.inject(AuthServiceService);
    httpMock = TestBed.inject(HttpTestingController);
    csrfStore = TestBed.inject(CsrfTokenStore);

    const antiforgeryReq = httpMock.expectOne(`${environment.api}/Account/antiforgery-token`);
    antiforgeryReq.flush({ token: 'bootstrap-csrf' });

    const meReq = httpMock.expectOne(`${environment.api}/Account/me`);
    meReq.flush(null, { status: 401, statusText: 'Unauthorized' });

    const clearSessionReq = httpMock.expectOne(`${environment.api}/Account/clear-session`);
    clearSessionReq.flush({ message: 'Session cleared.' });
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getHeaders should not attach Authorization bearer token', () => {
    expect(service.getHeaders().has('Authorization')).toBeFalse();
  });

  it('getHttpOptions should include withCredentials', () => {
    expect(service.getHttpOptions().withCredentials).toBeTrue();
  });

  it('login should use cookie auth and return the signed-in user profile', () => {
    service.login('user@example.com', 'Password123!').subscribe((user) => {
      expect(user?.userName).toBe('test-user');
      expect(service.isRole('user')).toBeTrue();
    });

    const csrfReq = httpMock.expectOne(`${environment.api}/Account/antiforgery-token`);
    csrfReq.flush({ token: 'login-csrf' });

    const loginReq = httpMock.expectOne(`${environment.api}/Account/login`);
    expect(loginReq.request.withCredentials).toBeTrue();
    expect(loginReq.request.headers.get('Authorization')).toBeNull();
    loginReq.flush({
      expiration: new Date().toISOString(),
      userId: 'user-1',
      email: 'user@example.com',
      userName: 'test-user',
      roles: ['user'],
    });

    expect(service.isRole('user')).toBeTrue();
  });

  it('logout should call backend logout endpoint', () => {
    service.logout().subscribe();

    const csrfReq = httpMock.expectOne(`${environment.api}/Account/antiforgery-token`);
    csrfReq.flush({ token: 'logout-csrf' });

    const logoutReq = httpMock.expectOne(`${environment.api}/Account/logout`);
    expect(logoutReq.request.withCredentials).toBeTrue();
    logoutReq.flush({ message: 'Logged out successfully' });

    expect(service.isAuthenticated()).toBeFalse();
    expect(csrfStore.getToken()).toBeNull();
  });
});

import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { credentialsInterceptor } from './interceptors/credentials.interceptor';
import { CsrfTokenStore } from './csrf-token.store';

describe('credentialsInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let csrfStore: CsrfTokenStore;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([credentialsInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    csrfStore = TestBed.inject(CsrfTokenStore);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should add withCredentials to GET requests', () => {
    http.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.withCredentials).toBeTrue();
    req.flush({});
  });

  it('should add CSRF header to mutating requests when token is present', () => {
    csrfStore.setToken('csrf-token');

    http.post('/api/test', {}).subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.withCredentials).toBeTrue();
    expect(req.request.headers.get('X-XSRF-TOKEN')).toBe('csrf-token');
    req.flush({});
  });
});

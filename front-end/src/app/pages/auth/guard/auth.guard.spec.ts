import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthGuard } from './auth.guard';
import { AuthServiceService } from '../auth-services/auth-service.service';

describe('AuthGuard', () => {
  let guard: AuthGuard;
  let authService: jasmine.SpyObj<AuthServiceService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthServiceService', ['isTokenExpired', 'logout']);
    router = jasmine.createSpyObj('Router', ['navigate']);
    authService.logout.and.returnValue(of(void 0));

    TestBed.configureTestingModule({
      providers: [
        AuthGuard,
        { provide: AuthServiceService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });

    guard = TestBed.inject(AuthGuard);
  });

  it('allows access when the session is valid', () => {
    authService.isTokenExpired.and.returnValue(false);

    expect(guard.canActivate()).toBeTrue();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('redirects to login when the session is expired', () => {
    authService.isTokenExpired.and.returnValue(true);

    expect(guard.canActivate()).toBeFalse();
    expect(authService.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
  });
});

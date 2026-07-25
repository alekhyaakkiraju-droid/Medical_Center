import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AdminGuard } from './admin.guard';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';

describe('AdminGuard', () => {
  let guard: AdminGuard;
  let authService: jasmine.SpyObj<AuthServiceService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthServiceService', ['isTokenExpired', 'isRole'], {
      isLoggedSubject: { next: jasmine.createSpy('next') },
    });
    router = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        AdminGuard,
        { provide: AuthServiceService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });

    guard = TestBed.inject(AdminGuard);
  });

  it('redirects unauthenticated users to login before checking role', () => {
    authService.isTokenExpired.and.returnValue(true);

    expect(guard.canActivate({} as any, { url: '/admin/dashboard' } as any)).toBeFalse();
    expect(authService.isRole).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
    expect(authService.isLoggedSubject.next).not.toHaveBeenCalled();
  });

  it('allows authenticated admin users', () => {
    authService.isTokenExpired.and.returnValue(false);
    authService.isRole.and.returnValue(true);

    expect(guard.canActivate({} as any, { url: '/admin/dashboard' } as any)).toBeTrue();
    expect(authService.isRole).toHaveBeenCalledWith('admin');
    expect(authService.isLoggedSubject.next).not.toHaveBeenCalled();
  });

  it('redirects authenticated non-admin users to the error page', () => {
    authService.isTokenExpired.and.returnValue(false);
    authService.isRole.and.returnValue(false);

    expect(guard.canActivate({} as any, { url: '/admin/dashboard' } as any)).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/pages/general/errorPage']);
  });
});

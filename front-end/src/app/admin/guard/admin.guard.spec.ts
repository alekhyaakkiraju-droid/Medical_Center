import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { AdminGuard } from './admin.guard';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';

describe('AdminGuard', () => {
  let guard: AdminGuard;
  let authService: jasmine.SpyObj<AuthServiceService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthServiceService', ['resolveSession', 'isRole'], {
      isLoggedSubject: { next: jasmine.createSpy('next') },
    });
    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree']);
    router.createUrlTree.and.callFake((commands: unknown[]) => ({ commands } as any));

    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders,
        AdminGuard,
        { provide: AuthServiceService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });

    guard = TestBed.inject(AdminGuard);
  });

  it('redirects unauthenticated users to login before checking role', (done) => {
    authService.resolveSession.and.returnValue(of(null));

    guard.canActivate({} as any, { url: '/admin/dashboard' } as any).subscribe((result) => {
      expect(result).toEqual({ commands: ['/auth/login'] } as any);
      expect(authService.isRole).not.toHaveBeenCalled();
      done();
    });
  });

  it('allows authenticated admin users', (done) => {
    authService.resolveSession.and.returnValue(of({ userId: '1', email: 'a', userName: 'a', roles: ['admin'] }));
    authService.isRole.and.returnValue(true);

    guard.canActivate({} as any, { url: '/admin/dashboard' } as any).subscribe((result) => {
      expect(result).toBeTrue();
      expect(authService.isRole).toHaveBeenCalledWith('admin');
      done();
    });
  });

  it('redirects authenticated non-admin users to the error page', (done) => {
    authService.resolveSession.and.returnValue(of({ userId: '1', email: 'a', userName: 'a', roles: ['user'] }));
    authService.isRole.and.returnValue(false);

    guard.canActivate({} as any, { url: '/admin/dashboard' } as any).subscribe((result) => {
      expect(result).toEqual({ commands: ['/pages/general/errorPage'] } as any);
      done();
    });
  });
});

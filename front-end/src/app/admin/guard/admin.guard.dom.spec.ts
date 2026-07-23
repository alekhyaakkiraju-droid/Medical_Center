import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AdminGuard } from './admin.guard';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';

describe('AdminGuard DOM side effects', () => {
  it('does not mutate auth state when checking role', () => {
    const authService = jasmine.createSpyObj('AuthServiceService', ['isTokenExpired', 'isRole'], {
      isLoggedSubject: { next: jasmine.createSpy('next') },
    });
    const router = jasmine.createSpyObj('Router', ['navigate']);

    authService.isTokenExpired.and.returnValue(false);
    authService.isRole.and.returnValue(true);

    TestBed.configureTestingModule({
      providers: [
        AdminGuard,
        { provide: AuthServiceService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });

    const guard = TestBed.inject(AdminGuard);
    expect(guard.canActivate({} as any, { url: '/admin/dashboard' } as any)).toBeTrue();
    expect(authService.isLoggedSubject.next).not.toHaveBeenCalled();
  });
});

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { AdminGuard } from './admin.guard';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';

describe('AdminGuard DOM side effects', () => {
  it('does not mutate auth state when checking role', (done) => {
    const authService = jasmine.createSpyObj('AuthServiceService', ['resolveSession', 'isRole'], {
      isLoggedSubject: { next: jasmine.createSpy('next') },
    });
    const router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree']);
    router.createUrlTree.and.callFake((commands: unknown[]) => ({ commands } as any));

    authService.resolveSession.and.returnValue(
      of({ userId: '1', email: 'admin@uat.careshift.local', userName: 'admin', roles: ['admin'] })
    );
    authService.isRole.and.returnValue(true);

    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, 
        AdminGuard,
        { provide: AuthServiceService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });

    const guard = TestBed.inject(AdminGuard);
    guard.canActivate({} as any, { url: '/admin/dashboard' } as any).subscribe((result) => {
      expect(result).toBeTrue();
      expect(authService.isLoggedSubject.next).not.toHaveBeenCalled();
      done();
    });
  });
});

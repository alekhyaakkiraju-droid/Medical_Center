import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { PatientGuard } from './patient.guard';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';

describe('PatientGuard', () => {
  let guard: PatientGuard;
  let authService: { resolveSession: jasmine.Spy; isRole: jasmine.Spy };
  let router: jasmine.SpyObj<Router>;

  beforeEach(waitForAsync(() => {
    authService = { resolveSession: jasmine.createSpy('resolveSession'), isRole: jasmine.createSpy('isRole') };
    router = jasmine.createSpyObj('Router', ['createUrlTree']);
    router.createUrlTree.and.callFake((commands: unknown[]) => ({ toString: () => commands.join('/') }) as never);
    TestBed.configureTestingModule({
      providers: [
        PatientGuard,
        { provide: AuthServiceService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });
    guard = TestBed.inject(PatientGuard);
  }));

  it('allows patient users', (done) => {
    authService.resolveSession.and.returnValue(of({ userId: '1', email: 'p@x.com', userName: 'pat', roles: ['user'] }));
    authService.isRole.and.returnValue(true);
    guard.canActivate().subscribe((result) => {
      expect(result).toBeTrue();
      done();
    });
  });

  it('redirects unauthenticated users to login', (done) => {
    authService.resolveSession.and.returnValue(of(null));
    guard.canActivate().subscribe(() => {
      expect(router.createUrlTree).toHaveBeenCalledWith(['/auth/login']);
      done();
    });
  });
});

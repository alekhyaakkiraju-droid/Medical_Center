import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { DoctorGuard } from './doctor.guard';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';

describe('DoctorGuard', () => {
  it('uses async session resolution', (done) => {
    const authService = {
      resolveSession: jasmine.createSpy('resolveSession').and.returnValue(of({ userId: '1', email: 'd@x.com', userName: 'doc', roles: ['doctor'] })),
      isRole: jasmine.createSpy('isRole').and.returnValue(true),
    };
    const router = jasmine.createSpyObj('Router', ['createUrlTree']);
    TestBed.configureTestingModule({
      providers: [
        DoctorGuard,
        { provide: AuthServiceService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });
    TestBed.inject(DoctorGuard).canActivate().subscribe((result) => {
      expect(authService.resolveSession).toHaveBeenCalled();
      expect(result).toBeTrue();
      done();
    });
  });
});

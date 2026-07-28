import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { nppGuard } from './npp.guard';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { NppService } from '../services/npp.service';
import { NppModalService } from '../services/npp-modal.service';

describe('nppGuard', () => {
  let authService: jasmine.SpyObj<AuthServiceService>;
  let nppService: jasmine.SpyObj<NppService>;
  let nppModalService: jasmine.SpyObj<NppModalService>;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthServiceService', ['isAuthenticated']);
    nppService = jasmine.createSpyObj('NppService', ['checkStatus', 'getContent', 'acknowledge']);
    nppModalService = jasmine.createSpyObj('NppModalService', ['show']);

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthServiceService, useValue: authService },
        { provide: NppService, useValue: nppService },
        { provide: NppModalService, useValue: nppModalService },
      ],
    });
  });

  it('allows unauthenticated users through', (done) => {
    authService.isAuthenticated.and.returnValue(false);

    TestBed.runInInjectionContext(() => {
      const result = nppGuard({} as any, {} as any);
      if (typeof result === 'boolean') {
        expect(result).toBeTrue();
        done();
      } else {
        result.subscribe((value) => {
          expect(value).toBeTrue();
          done();
        });
      }
    });
  });

  it('allows acknowledged users through', (done) => {
    authService.isAuthenticated.and.returnValue(true);
    nppService.checkStatus.and.returnValue(of({ acknowledged: true, version: '1.0' }));

    TestBed.runInInjectionContext(() => {
      (nppGuard({} as any, {} as any) as any).subscribe((value: boolean) => {
        expect(value).toBeTrue();
        done();
      });
    });
  });

  it('blocks until acknowledgment succeeds', (done) => {
    authService.isAuthenticated.and.returnValue(true);
    nppService.checkStatus.and.returnValue(of({ acknowledged: false, version: '1.0' }));
    nppService.getContent.and.returnValue(of({ content: 'NPP', version: '1.0', lastUpdated: 'now' }));
    nppModalService.show.and.returnValue(Promise.resolve(true));
    nppService.acknowledge.and.returnValue(of(void 0));

    TestBed.runInInjectionContext(() => {
      (nppGuard({} as any, {} as any) as any).subscribe((value: boolean) => {
        expect(value).toBeTrue();
        expect(nppService.acknowledge).toHaveBeenCalled();
        done();
      });
    });
  });
});

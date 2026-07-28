import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { PLATFORM_ID } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import {
  IDLE_TIMEOUT_CONFIG,
  IdleTimeoutConfig,
} from './idle-timeout-config';
import { IdleTimeoutService } from './idle-timeout.service';

describe('IdleTimeoutService', () => {
  let service: IdleTimeoutService;
  let authStatus$: BehaviorSubject<boolean>;
  let config: IdleTimeoutConfig;

  beforeEach(() => {
    authStatus$ = new BehaviorSubject<boolean>(false);
    config = {
      idleDurationMs: 5_000,
      warningDurationMs: 3_000,
    };

    TestBed.configureTestingModule({
      providers: [
        IdleTimeoutService,
        {
          provide: PLATFORM_ID,
          useValue: 'browser',
        },
        {
          provide: IDLE_TIMEOUT_CONFIG,
          useValue: config,
        },
        {
          provide: AuthServiceService,
          useValue: {
            getloggedStatus: () => authStatus$.asObservable(),
          },
        },
      ],
    });

    service = TestBed.inject(IdleTimeoutService);
  });

  afterEach(() => {
    service.ngOnDestroy();
  });

  it('resets timer on user activity before idle threshold', fakeAsync(() => {
    authStatus$.next(true);
    service.start();

    document.dispatchEvent(new Event('mousemove'));
    tick(4_000);
    document.dispatchEvent(new Event('keydown'));
    tick(4_000);

    let latestState = 'idle';
    service.state$.subscribe((status) => {
      latestState = status.state;
    });

    expect(latestState).toBe('idle');
  }));

  it('shows warning after idle period', fakeAsync(() => {
    authStatus$.next(true);
    service.start();

    document.dispatchEvent(new Event('mousemove'));
    tick(5_000);

    let latestState = 'idle';
    service.state$.subscribe((status) => {
      latestState = status.state;
    });

    expect(latestState).toBe('warning');
  }));

  it('decrements countdown during warning period', fakeAsync(() => {
    authStatus$.next(true);
    service.start();

    document.dispatchEvent(new Event('mousemove'));
    tick(5_000);

    let countdown = 0;
    service.state$.subscribe((status) => {
      if (status.countdownSeconds !== null) {
        countdown = status.countdownSeconds;
      }
    });

    expect(countdown).toBe(3);
    tick(1_000);
    expect(countdown).toBe(2);
  }));

  it('emits timeout when warning expires', fakeAsync(() => {
    authStatus$.next(true);
    service.start();

    document.dispatchEvent(new Event('mousemove'));
    tick(5_000);
    tick(3_000);

    let latestState = 'idle';
    service.state$.subscribe((status) => {
      latestState = status.state;
    });

    expect(latestState).toBe('timeout');
  }));

  it('cancels warning when resetTimer is called', fakeAsync(() => {
    authStatus$.next(true);
    service.start();

    document.dispatchEvent(new Event('mousemove'));
    tick(5_000);
    service.resetTimer();

    let latestState = 'warning';
    service.state$.subscribe((status) => {
      latestState = status.state;
    });

    tick(3_000);
    expect(latestState).toBe('idle');
  }));

  it('does not start on server platform', () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        IdleTimeoutService,
        {
          provide: PLATFORM_ID,
          useValue: 'server',
        },
        {
          provide: IDLE_TIMEOUT_CONFIG,
          useValue: config,
        },
        {
          provide: AuthServiceService,
          useValue: {
            getloggedStatus: () => authStatus$.asObservable(),
          },
        },
      ],
    });

    const serverService = TestBed.inject(IdleTimeoutService);
    authStatus$.next(true);
    serverService.start();

    let latestState = 'idle';
    serverService.state$.subscribe((status) => {
      latestState = status.state;
    });

    expect(latestState).toBe('idle');
    serverService.ngOnDestroy();
  });
});

import { Inject, Injectable, OnDestroy, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import {
  BehaviorSubject,
  fromEvent,
  merge,
  Observable,
  Subject,
  Subscription,
  timer,
} from 'rxjs';
import { map, switchMap, takeUntil, takeWhile, tap } from 'rxjs/operators';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import {
  DEFAULT_IDLE_TIMEOUT_CONFIG,
  IDLE_TIMEOUT_CONFIG,
  IdleTimeoutConfig,
} from './idle-timeout-config';

export type IdleTimeoutState = 'idle' | 'warning' | 'timeout';

export interface IdleTimeoutStatus {
  state: IdleTimeoutState;
  countdownSeconds: number | null;
}

const ACTIVITY_EVENTS = ['mousemove', 'keydown', 'scroll', 'touchstart'] as const;

@Injectable({
  providedIn: 'root',
})
export class IdleTimeoutService implements OnDestroy {
  private readonly statusSubject = new BehaviorSubject<IdleTimeoutStatus>({
    state: 'idle',
    countdownSeconds: null,
  });
  private readonly resetSubject = new Subject<void>();
  private readonly destroySubject = new Subject<void>();

  private activitySubscription: Subscription | null = null;
  private authSubscription: Subscription | null = null;
  private visibilitySubscription: Subscription | null = null;
  private lastActivityAt = 0;
  private warningStartedAt = 0;
  private running = false;

  readonly state$ = this.statusSubject.asObservable();

  constructor(
    @Inject(PLATFORM_ID) private readonly platformId: object,
    @Inject(IDLE_TIMEOUT_CONFIG) private readonly config: IdleTimeoutConfig,
    private readonly authService: AuthServiceService
  ) {
    if (this.isBrowser) {
      this.authSubscription = this.authService.getloggedStatus().subscribe((isLoggedIn) => {
        if (isLoggedIn) {
          this.start();
        } else {
          this.stop();
        }
      });
    }
  }

  get isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  start(): void {
    if (!this.isBrowser || this.running) {
      return;
    }

    this.running = true;
    this.lastActivityAt = Date.now();
    this.bindVisibilityListener();
    this.subscribeToActivity();
  }

  stop(): void {
    this.running = false;
    this.activitySubscription?.unsubscribe();
    this.activitySubscription = null;
    this.visibilitySubscription?.unsubscribe();
    this.visibilitySubscription = null;
    this.resetSubject.next();
    this.emitStatus('idle', null);
  }

  resetTimer(): void {
    if (!this.isBrowser || !this.running) {
      return;
    }

    this.lastActivityAt = Date.now();
    this.warningStartedAt = 0;
    this.resetSubject.next();
    this.emitStatus('idle', null);
    this.subscribeToActivity();
  }

  ngOnDestroy(): void {
    this.stop();
    this.authSubscription?.unsubscribe();
    this.destroySubject.next();
    this.destroySubject.complete();
    this.statusSubject.complete();
  }

  private subscribeToActivity(): void {
    this.activitySubscription?.unsubscribe();
    this.resetSubject.next();

    const activity$ = merge(
      ...ACTIVITY_EVENTS.map((eventName) => fromEvent(document, eventName))
    ).pipe(
      tap(() => {
        this.lastActivityAt = Date.now();
      }),
      takeUntil(this.resetSubject),
      takeUntil(this.destroySubject)
    );

    this.activitySubscription = activity$
      .pipe(
        switchMap(() => this.createIdleSequence$()),
        takeUntil(this.resetSubject),
        takeUntil(this.destroySubject)
      )
      .subscribe();
  }

  private createIdleSequence$(): Observable<void> {
    return timer(this.config.idleDurationMs).pipe(
      tap(() => {
        this.warningStartedAt = Date.now();
        this.emitStatus('warning', Math.ceil(this.config.warningDurationMs / 1000));
      }),
      switchMap(() =>
        timer(0, 1000).pipe(
          map(() => {
            const elapsedMs = Date.now() - this.warningStartedAt;
            const remainingMs = this.config.warningDurationMs - elapsedMs;
            return Math.max(0, Math.ceil(remainingMs / 1000));
          }),
          tap((remainingSeconds) => {
            if (remainingSeconds <= 0) {
              this.emitStatus('timeout', 0);
            } else {
              this.emitStatus('warning', remainingSeconds);
            }
          }),
          takeWhile((remainingSeconds) => remainingSeconds > 0, true),
          map(() => void 0)
        )
      )
    );
  }

  private bindVisibilityListener(): void {
    if (this.visibilitySubscription) {
      return;
    }

    this.visibilitySubscription = fromEvent(document, 'visibilitychange')
      .pipe(takeUntil(this.destroySubject))
      .subscribe(() => {
        if (document.visibilityState !== 'visible' || !this.running) {
          return;
        }

        const now = Date.now();
        const idleElapsedMs = now - this.lastActivityAt;

        if (this.warningStartedAt > 0) {
          const warningElapsedMs = now - this.warningStartedAt;
          if (warningElapsedMs >= this.config.warningDurationMs) {
            this.emitStatus('timeout', 0);
            return;
          }

          const remainingSeconds = Math.ceil(
            (this.config.warningDurationMs - warningElapsedMs) / 1000
          );
          this.emitStatus('warning', remainingSeconds);
          return;
        }

        if (idleElapsedMs >= this.config.idleDurationMs + this.config.warningDurationMs) {
          this.emitStatus('timeout', 0);
          return;
        }

        if (idleElapsedMs >= this.config.idleDurationMs) {
          this.warningStartedAt = this.lastActivityAt + this.config.idleDurationMs;
          const warningElapsedMs = now - this.warningStartedAt;
          const remainingSeconds = Math.ceil(
            (this.config.warningDurationMs - warningElapsedMs) / 1000
          );
          this.emitStatus('warning', Math.max(remainingSeconds, 0));
        }
      });
  }

  private emitStatus(state: IdleTimeoutState, countdownSeconds: number | null): void {
    this.statusSubject.next({ state, countdownSeconds });
  }
}

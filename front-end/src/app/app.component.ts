import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { ReloadService } from './shared/service/reload.service';
import { NgIf } from '@angular/common';
import { HeaderComponent } from './layout/header/header.component';
import { FooterComponent } from './layout/footer/footer.component';
import { SkipToContentComponent } from './shared/components/skip-to-content/skip-to-content.component';
import { RouteAnnouncerService } from './shared/services/route-announcer.service';
import { IdleTimeoutService } from './core/services/idle-timeout.service';
import { SessionTimeoutWarningComponent } from './core/components/session-timeout-warning/session-timeout-warning.component';
import { NppAcknowledgmentComponent } from './core/components/npp-acknowledgment/npp-acknowledgment.component';
import { AuthServiceService } from './pages/auth/auth-services/auth-service.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  imports: [NgIf, HeaderComponent, FooterComponent, RouterOutlet, SkipToContentComponent, SessionTimeoutWarningComponent, NppAcknowledgmentComponent],
})
export class AppComponent implements OnInit, AfterViewInit, OnDestroy {
  title = 'MedicalCenter';
  showHeaderAndNavbar = true;
  showSessionTimeoutWarning = false;
  sessionTimeoutCountdown: number | null = null;
  routeAnnouncement = '';

  @ViewChild('mainContent') mainContent?: ElementRef<HTMLElement>;

  private idleSubscription?: Subscription;
  private routeSubscription?: Subscription;

  constructor(
    private router: Router,
    private reload: ReloadService,
    private idleTimeoutService: IdleTimeoutService,
    private authService: AuthServiceService,
    private routeAnnouncer: RouteAnnouncerService
  ) {}

  ngAfterViewInit(): void {
    this.hideRoutePreloader();
  }

  ngOnInit(): void {
    this.updateChromeVisibility(this.router.url);
    this.routeSubscription = this.routeAnnouncer.announcement$.subscribe((text) => {
      this.routeAnnouncement = text;
    });

    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => {
        this.updateChromeVisibility(event.urlAfterRedirects);
        this.routeAnnouncer.focusMainContent(this.mainContent?.nativeElement);
        setTimeout(() => this.hideRoutePreloader(), 0);
      });

    this.idleSubscription = this.idleTimeoutService.state$.subscribe((status) => {
      this.showSessionTimeoutWarning = status.state === 'warning';
      this.sessionTimeoutCountdown = status.countdownSeconds;

      if (status.state === 'timeout') {
        this.authService.sessionTimeout().subscribe();
      }
    });
  }

  ngOnDestroy(): void {
    this.idleSubscription?.unsubscribe();
    this.routeSubscription?.unsubscribe();
  }

  onStayLoggedIn(): void {
    this.authService.refreshToken().subscribe({
      next: () => this.idleTimeoutService.resetTimer(),
      error: () => this.idleTimeoutService.resetTimer(),
    });
  }

  onSessionTimeoutLogOut(): void {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/auth/login']);
    });
  }

  private updateChromeVisibility(url: string): void {
    this.showHeaderAndNavbar =
      !url.includes('/admin') &&
      !url.includes('/doctor') &&
      !url.includes('/error') &&
      !url.includes('/auth');
  }

  private hideRoutePreloader(): void {
    this.reload.initializeLoader();
  }
}

import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { BehaviorSubject, filter } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class RouteAnnouncerService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly announcementSubject = new BehaviorSubject<string>('');

  readonly announcement$ = this.announcementSubject.asObservable();

  constructor() {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => this.announcementSubject.next(this.resolvePageTitle()));
  }

  resolvePageTitle(): string {
    if (!isPlatformBrowser(this.platformId)) {
      return '';
    }

    let active = this.route.root;
    while (active.firstChild) {
      active = active.firstChild;
    }

    const routeTitle = active.snapshot.data['title'];
    if (typeof routeTitle === 'string' && routeTitle.trim().length > 0) {
      return routeTitle;
    }

    return document.title || 'CareShift';
  }
}

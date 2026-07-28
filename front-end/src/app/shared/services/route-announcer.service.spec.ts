import { TestBed } from '@angular/core/testing';
import { Router, NavigationEnd } from '@angular/router';
import { Subject } from 'rxjs';
import { RouteAnnouncerService } from './route-announcer.service';
import { ActivatedRoute } from '@angular/router';

describe('RouteAnnouncerService', () => {
  let service: RouteAnnouncerService;
  let routerEvents: Subject<unknown>;
  let routeData: Record<string, unknown>;

  beforeEach(() => {
    routerEvents = new Subject();
    routeData = { title: 'Home - CareShift' };

    TestBed.configureTestingModule({
      providers: [
        RouteAnnouncerService,
        {
          provide: Router,
          useValue: {
            events: routerEvents.asObservable(),
            routerState: { snapshot: { root: {} } },
          },
        },
        {
          provide: ActivatedRoute,
          useValue: {
            root: {
              firstChild: {
                firstChild: null,
                snapshot: { data: routeData },
              },
              snapshot: { data: {} },
            },
          },
        },
      ],
    });

    service = TestBed.inject(RouteAnnouncerService);
  });

  it('updates announcement text on NavigationEnd events', (done) => {
    service.announcement$.subscribe((text) => {
      if (text) {
        expect(text).toBe('Home - CareShift');
        done();
      }
    });

    routerEvents.next(new NavigationEnd(1, '/', '/'));
  });

  it('resolvePageTitle returns route data title', () => {
    expect(service.resolvePageTitle()).toBe('Home - CareShift');
  });
});

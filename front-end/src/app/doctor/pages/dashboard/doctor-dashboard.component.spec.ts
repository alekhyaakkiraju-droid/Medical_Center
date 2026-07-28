import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { DoctorDashboardComponent } from './doctor-dashboard.component';
import { DoctorAppointmentsService } from '../../services/doctor-appointments.service';
import { AuthServiceService } from '../../../pages/auth/auth-services/auth-service.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';
import { emptyTodaysBookings, multipleTodaysBookings, singleTodaysBooking, upcomingBookingsFixture } from '../../../testing/fixtures/doctor-bookings.mock';

describe('DoctorDashboardComponent', () => {
  let component: DoctorDashboardComponent;
  let fixture: ComponentFixture<DoctorDashboardComponent>;
  let svc: { getTodaysBookings: jasmine.Spy; getUpcomingBookings: jasmine.Spy };

  beforeEach(waitForAsync(() => {
    svc = jasmine.createSpyObj('DoctorAppointmentsService', ['getTodaysBookings', 'getUpcomingBookings']);
    TestBed.configureTestingModule({
      imports: [DoctorDashboardComponent],
      providers: [
        ...standaloneComponentTestProviders,
        { provide: DoctorAppointmentsService, useValue: svc },
        { provide: AuthServiceService, useValue: { getNameIdentifier: () => 'doctor-1', getUserName: () => 'Dr. Smith', getUsernameFromToken: () => 'Dr. Smith' } },
        { provide: ReloadService, useValue: { initializeLoader: () => undefined } },
      ],
    }).compileComponents();
  }));

  beforeEach(() => {
    svc.getTodaysBookings.and.returnValue(of(multipleTodaysBookings));
    svc.getUpcomingBookings.and.returnValue(of(upcomingBookingsFixture));
    fixture = TestBed.createComponent(DoctorDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());
  it('loads today and upcoming bookings on init', () => {
    expect(svc.getTodaysBookings).toHaveBeenCalledWith('doctor-1');
    expect(component.todaysBookings.length).toBe(3);
    expect(component.upcomingCount).toBe(2);
  });
  it('renders empty state when no today bookings exist', () => {
    svc.getTodaysBookings.and.returnValue(of(emptyTodaysBookings));
    component.loadDashboardData();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.doctor-dashboard__state p')?.textContent).toContain('No appointments scheduled for today');
  });
  it('sets loadError when API calls fail', () => {
    svc.getTodaysBookings.and.returnValue(throwError(() => new Error('network')));
    component.loadDashboardData();
    expect(component.loadError).toBeTrue();
  });
  it('exposes view-all link to appointments list', () => {
    expect(fixture.nativeElement.querySelector('a[routerLink="/doctor/doctor-appointments"]')?.textContent).toContain('View All');
  });
  it('retries loading after an error', () => {
    svc.getTodaysBookings.and.returnValue(throwError(() => new Error('network')));
    component.loadDashboardData();
    svc.getTodaysBookings.and.returnValue(of(singleTodaysBooking));
    component.retryLoad();
    expect(component.todaysBookings.length).toBe(1);
  });
});

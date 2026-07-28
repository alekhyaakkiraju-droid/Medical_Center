import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { PatientHomeComponent } from './patient-home.component';
import { AppointmentService } from '../../../pages/general/services/appointment.service';
import { AuthServiceService } from '../../../pages/auth/auth-services/auth-service.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';
import { emptyPatientAppointments, mixedPatientAppointments, upcomingPatientAppointments } from '../../../testing/fixtures/patient-appointments.mock';

describe('PatientHomeComponent', () => {
  let component: PatientHomeComponent;
  let fixture: ComponentFixture<PatientHomeComponent>;
  let appointmentService: { getUserAppointments: jasmine.Spy };

  beforeEach(waitForAsync(() => {
    appointmentService = jasmine.createSpyObj('AppointmentService', ['getUserAppointments']);
    TestBed.configureTestingModule({
      imports: [PatientHomeComponent],
      providers: [
        ...standaloneComponentTestProviders,
        { provide: AppointmentService, useValue: appointmentService },
        { provide: AuthServiceService, useValue: { getUserName: () => 'Alice' } },
        { provide: ReloadService, useValue: { initializeLoader: () => undefined } },
      ],
    }).compileComponents();
  }));

  beforeEach(() => {
    appointmentService.getUserAppointments.and.returnValue(of(upcomingPatientAppointments));
    fixture = TestBed.createComponent(PatientHomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());
  it('loads upcoming appointments on init', () => {
    expect(appointmentService.getUserAppointments).toHaveBeenCalled();
    expect(component.upcomingAppointments.length).toBeGreaterThan(0);
  });
  it('shows empty state when no appointments', () => {
    appointmentService.getUserAppointments.and.returnValue(of(emptyPatientAppointments));
    component.loadAppointments();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('You have no upcoming appointments');
  });
  it('shows book appointment CTA in empty state', () => {
    appointmentService.getUserAppointments.and.returnValue(of(emptyPatientAppointments));
    component.loadAppointments();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.patient-home__cta')).toBeTruthy();
  });
  it('sets loadError when API fails', () => {
    appointmentService.getUserAppointments.and.returnValue(throwError(() => new Error('fail')));
    component.loadAppointments();
    expect(component.loadError).toBeTrue();
  });
  it('limits recent visits to completed/cancelled entries', () => {
    appointmentService.getUserAppointments.and.returnValue(of(mixedPatientAppointments));
    component.loadAppointments();
    expect(component.recentVisits.length).toBe(2);
  });
});

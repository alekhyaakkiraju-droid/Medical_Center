import { waitForAsync, ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { RelatedAppointmentsComponent } from './related-appointments.component';
import { DoctorAppointmentsService } from '../../services/doctor-appointments.service';
import { AuthServiceService } from '../../../pages/auth/auth-services/auth-service.service';
import { SearchService } from '../../services/search.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { ToastrService } from 'ngx-toastr';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';
import { mockBookingDTO, mockPagedResult } from '../../../../testing/mock-data';

describe('RelatedAppointmentsComponent', () => {
  let component: RelatedAppointmentsComponent;
  let fixture: ComponentFixture<RelatedAppointmentsComponent>;
  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [RelatedAppointmentsComponent],
      providers: [
        ...standaloneComponentTestProviders,
        { provide: DoctorAppointmentsService, useValue: { getAllDoctorBookings: () => of(mockPagedResult([mockBookingDTO()])), deleteBooking: () => of(undefined) } },
        { provide: AuthServiceService, useValue: { getNameIdentifier: () => 'doctor-1' } },
        { provide: SearchService, useValue: { search: () => undefined } },
        { provide: ReloadService, useValue: { initializeLoader: () => undefined } },
        { provide: ToastrService, useValue: { success: () => undefined, error: () => undefined } },
      ],
    }).compileComponents();
  }));
  beforeEach(() => { fixture = TestBed.createComponent(RelatedAppointmentsComponent); component = fixture.componentInstance; fixture.detectChanges(); });
  it('should create', () => { expect(component).toBeTruthy(); });
  it('renders booking list data from the doctor service', () => { expect(component.allBookings.length).toBe(1); expect(component.displayBookings[0].patientName).toBe('Test Patient'); });
  it('filters bookings to today when selected', () => {
    component.allBookings = [mockBookingDTO({ appointmentTakenDate: new Date().toISOString() }), mockBookingDTO({ appointmentId: 2, appointmentTakenDate: '2020-01-01T00:00:00Z' })];
    component.onFilterChange('2');
    expect(component.displayBookings.length).toBe(1);
  });
});

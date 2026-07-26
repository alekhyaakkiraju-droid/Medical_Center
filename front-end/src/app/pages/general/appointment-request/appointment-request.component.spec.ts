import { waitForAsync, ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AppointmentRequestComponent } from './appointment-request.component';
import { SpecializationService } from '../services/specialization.service';
import { DoctorService } from '../services/doctor.service';
import { AppointmentService } from '../services/appointment.service';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';
import { mockDoctorDTO, mockPagedResult } from '../../../../testing/mock-data';

describe('AppointmentRequestComponent', () => {
  let component: AppointmentRequestComponent;
  let fixture: ComponentFixture<AppointmentRequestComponent>;
  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [AppointmentRequestComponent],
      providers: [
        ...standaloneComponentTestProviders,
        { provide: SpecializationService, useValue: { getSpecializations: () => of(mockPagedResult([{ id: 1, name: 'Cardiology' }])) } },
        { provide: DoctorService, useValue: { getAllDoctors: () => of(mockPagedResult([mockDoctorDTO()])) } },
        { provide: AppointmentService, useValue: { createAppointment: () => of({}) } },
        { provide: AuthServiceService, useValue: { getloggedStatus: () => of(true), getNameIdentifier: () => 'patient-1' } },
        { provide: Router, useValue: { navigate: () => Promise.resolve(true) } },
        { provide: ToastrService, useValue: { success: () => undefined, error: () => undefined } },
      ],
    }).compileComponents();
  }));
  beforeEach(() => { fixture = TestBed.createComponent(AppointmentRequestComponent); component = fixture.componentInstance; fixture.detectChanges(); });
  it('should create', () => { expect(component).toBeTruthy(); });
  it('loads doctors for the selection dropdown', () => { expect(component.doctorsData.length).toBe(1); expect(component.doctorsData[0].name).toBe('Dr. Contract Test'); });
  it('requires doctor selection before submit', () => {
    component.appointmentForm.patchValue({ name: 'Jane Doe', email: 'jane@example.com', phone: '5551234567', date: '2026-08-01', department: 'Cardiology', doctor: '', message: 'Need an appointment please' });
    expect(component.appointmentForm.valid).toBeFalse();
  });
});

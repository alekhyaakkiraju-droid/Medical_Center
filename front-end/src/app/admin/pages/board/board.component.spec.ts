import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { of } from 'rxjs';
import { BoardComponent } from './board.component';
import { AppointmentService } from '../../../pages/general/services/appointment.service';
import { DoctorService } from '../../../pages/general/services/doctor.service';
import { PatientService } from '../../services/patient.service';
import { TotalEarningsService } from '../../services/total-earnings.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { ToastrService } from 'ngx-toastr';
import { AuthServiceService } from '../../../pages/auth/auth-services/auth-service.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';
import { mockAppointmentDTO, mockDoctorDTO, mockPagedResult, mockPatientDTO } from '../../../../testing/mock-data';

describe('BoardComponent', () => {
  let component: BoardComponent;
  let fixture: ComponentFixture<BoardComponent>;
  let appointmentService: { getAppointments: jasmine.Spy };
  let doctorService: { getAllDoctors: jasmine.Spy };
  let patientService: { getAllPatient: jasmine.Spy };
  let earningsService: { getTotalEarnings: jasmine.Spy };

  beforeEach(waitForAsync(() => {
    appointmentService = jasmine.createSpyObj('AppointmentService', ['getAppointments']);
    doctorService = jasmine.createSpyObj('DoctorService', ['getAllDoctors']);
    patientService = jasmine.createSpyObj('PatientService', ['getAllPatient']);
    earningsService = jasmine.createSpyObj('TotalEarningsService', ['getTotalEarnings']);

    TestBed.configureTestingModule({
      imports: [BoardComponent],
      providers: [
        ...standaloneComponentTestProviders,
        { provide: AppointmentService, useValue: appointmentService },
        { provide: DoctorService, useValue: doctorService },
        { provide: PatientService, useValue: patientService },
        { provide: TotalEarningsService, useValue: earningsService },
        { provide: AuthServiceService, useValue: { resolveSession: () => of({ userId: '1', email: 'admin@uat.careshift.local', userName: 'admin', roles: ['admin'] }), isRole: () => true, getUserName: () => 'admin', getUsernameFromToken: () => 'admin' } },
        { provide: ReloadService, useValue: { initializeLoader: () => undefined } },
        { provide: ToastrService, useValue: { success: () => undefined, error: () => undefined } },
      ],
    }).compileComponents();
  }));

  beforeEach(() => {
    appointmentService.getAppointments.and.returnValue(of(mockPagedResult([mockAppointmentDTO()])));
    doctorService.getAllDoctors.and.returnValue(of(mockPagedResult([mockDoctorDTO()], { totalCount: 1 })));
    patientService.getAllPatient.and.returnValue(of(mockPagedResult([mockPatientDTO()], { totalCount: 1 })));
    earningsService.getTotalEarnings.and.returnValue(of({ totalEarnings: 5000 }));
    fixture = TestBed.createComponent(BoardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => { expect(component).toBeTruthy(); });

  it('shows appointments empty state when API returns empty array', () => {
    appointmentService.getAppointments.and.returnValue(of(mockPagedResult([])));
    component.loadAppointments();
    fixture.detectChanges();
    expect(component.hasAppointments).toBeFalse();
    expect(fixture.nativeElement.textContent).toContain('No appointments booked yet');
  });

  it('shows doctors empty state when API returns empty array', () => {
    doctorService.getAllDoctors.and.returnValue(of(mockPagedResult([])));
    component.loadDoctor();
    fixture.detectChanges();
    expect(component.hasDoctors).toBeFalse();
    expect(fixture.nativeElement.textContent).toContain('No doctors registered');
  });

  it('marks earnings empty when total is zero', () => {
    earningsService.getTotalEarnings.and.returnValue(of({ totalEarnings: 0 }));
    component.getTotalEarning();
    expect(component.hasEarnings).toBeFalse();
  });
});

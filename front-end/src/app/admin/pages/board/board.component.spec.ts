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
  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [BoardComponent],
      providers: [
        ...standaloneComponentTestProviders,
        { provide: AppointmentService, useValue: { getAppointments: () => of(mockPagedResult([mockAppointmentDTO()])) } },
        { provide: DoctorService, useValue: { getAllDoctors: () => of(mockPagedResult([mockDoctorDTO()], { totalCount: 1 })) } },
        { provide: PatientService, useValue: { getAllPatient: () => of(mockPagedResult([mockPatientDTO(), mockPatientDTO({ patientId: 'patient-2' })], { totalCount: 2 })) } },
        { provide: TotalEarningsService, useValue: { getTotalEarnings: () => of({ totalEarnings: 5000 }) } },
        { provide: AuthServiceService, useValue: { resolveSession: () => of({ userId: '1', email: 'admin@uat.careshift.local', userName: 'admin', roles: ['admin'] }), isRole: () => true } },
        { provide: ReloadService, useValue: { initializeLoader: () => undefined } },
        { provide: ToastrService, useValue: { success: () => undefined, error: () => undefined } },
      ],
    }).compileComponents();
  }));
  beforeEach(() => { fixture = TestBed.createComponent(BoardComponent); component = fixture.componentInstance; fixture.detectChanges(); });
  it('should create', () => { expect(component).toBeTruthy(); });
  it('binds appointment list data from the service', () => {
    expect(component.appointments.length).toBe(1);
    expect(component.numOfAppointments).toBe(1);
  });
  it('binds patient count and earnings widgets', () => {
    expect(component.numOfPatients).toBe(2);
    expect(component.totalAmountEarning).toBe(5000);
    expect(component.infoBoxes.find((box) => box.text === 'Total Earnings')?.number).toBe(5000);
  });
});

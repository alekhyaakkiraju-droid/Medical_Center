import { Component, NO_ERRORS_SCHEMA, ChangeDetectionStrategy } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

import { LoginComponent } from '../pages/auth/login/login.component';
import { RegisterComponent } from '../pages/auth/register/register.component';
import { AppointmentRequestComponent } from '../pages/general/appointment-request/appointment-request.component';
import { BoardComponent } from '../admin/pages/board/board.component';
import { AuthServiceService } from '../pages/auth/auth-services/auth-service.service';
import { ReloadService } from '../shared/service/reload.service';
import { ForgotServiceService } from '../pages/auth/auth-services/forgot-service.service';
import { ResetPasswordService } from '../pages/auth/auth-services/resetPassword.service';
import { ModelService } from '../pages/auth/auth-services/model.service';
import { SpecializationService } from '../pages/general/services/specialization.service';
import { DoctorService } from '../pages/general/services/doctor.service';
import { AppointmentService } from '../pages/general/services/appointment.service';
import { PatientService } from '../admin/services/patient.service';
import { TotalEarningsService } from '../admin/services/total-earnings.service';
import { expectNoA11yViolations } from './axe-test.helper';
import { standaloneComponentTestProviders } from '../testing/standalone-component-test-providers';

@Component({
    selector: 'app-forgetPassword', template: '', changeDetection: ChangeDetectionStrategy.Eager,
    imports: [ReactiveFormsModule, FormsModule, CommonModule]
})
class ForgetPasswordStubComponent {}

@Component({
    selector: 'app-side-bar', template: '', changeDetection: ChangeDetectionStrategy.Eager,
    imports: [ReactiveFormsModule, FormsModule, CommonModule]
})
class SideBarStubComponent {}

@Component({
    selector: 'app-chart', template: '', changeDetection: ChangeDetectionStrategy.Eager,
    imports: [ReactiveFormsModule, FormsModule, CommonModule]
})
class ChartStubComponent {}

@Component({
    selector: 'app-delete-modal', template: '', changeDetection: ChangeDetectionStrategy.Eager,
    imports: [ReactiveFormsModule, FormsModule, CommonModule]
})
class DeleteModalStubComponent {}

describe('WCAG 2.1 AA accessibility', () => {
  const reloadStub = { initializeLoader: () => {} };
  const routerStub = jasmine.createSpyObj('Router', ['navigate']);
  const toastrStub = jasmine.createSpyObj('ToastrService', ['success', 'error', 'info', 'warning']);

  describe('Login page', () => {
    let fixture: ComponentFixture<LoginComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
    imports: [ReactiveFormsModule, FormsModule, CommonModule, LoginComponent, ForgetPasswordStubComponent],
    providers: [...standaloneComponentTestProviders, 
        { provide: Router, useValue: routerStub },
        { provide: ToastrService, useValue: toastrStub },
        { provide: ReloadService, useValue: reloadStub },
        { provide: AuthServiceService, useValue: jasmine.createSpyObj('AuthServiceService', ['login', 'isRole', 'getUsernameFromToken']) },
        { provide: ForgotServiceService, useValue: jasmine.createSpyObj('ForgotServiceService', ['forgetPassword']) },
        { provide: ResetPasswordService, useValue: {} },
        { provide: ModelService, useValue: { dialogState$: of(false), openDialog: () => { }, closeDialog: () => { } } },
    ],
}).compileComponents();

      fixture = TestBed.createComponent(LoginComponent);
      fixture.detectChanges();
    });

    it('should have no critical WCAG 2.1 AA violations', async () => {
      await expectNoA11yViolations(fixture.nativeElement);
    });

    it('should associate labels with login form inputs', () => {
      const email = fixture.nativeElement.querySelector('#login-email');
      const password = fixture.nativeElement.querySelector('#login-password');
      expect(email?.getAttribute('aria-label')).toBe('Email address');
      expect(password?.getAttribute('aria-label')).toBe('Password');
    });
  });

  describe('Registration page', () => {
    let fixture: ComponentFixture<RegisterComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
    imports: [ReactiveFormsModule, FormsModule, CommonModule, RegisterComponent],
    providers: [...standaloneComponentTestProviders, 
        { provide: Router, useValue: routerStub },
        { provide: ToastrService, useValue: toastrStub },
        { provide: ReloadService, useValue: reloadStub },
        { provide: AuthServiceService, useValue: jasmine.createSpyObj('AuthServiceService', ['register']) },
    ],
}).compileComponents();

      fixture = TestBed.createComponent(RegisterComponent);
      fixture.detectChanges();
    });

    it('should have no critical WCAG 2.1 AA violations', async () => {
      await expectNoA11yViolations(fixture.nativeElement);
    });

    it('should label all registration inputs', () => {
      expect(fixture.nativeElement.querySelector('#register-full-name')?.getAttribute('aria-label')).toBe('Full name');
      expect(fixture.nativeElement.querySelector('#register-email')?.getAttribute('aria-label')).toBe('Email address');
      expect(fixture.nativeElement.querySelector('#register-password')?.getAttribute('aria-label')).toBe('Password');
      expect(fixture.nativeElement.querySelector('#register-confirm-password')?.getAttribute('aria-label')).toBe('Confirm password');
    });
  });

  describe('Appointment booking page', () => {
    let fixture: ComponentFixture<AppointmentRequestComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
    imports: [ReactiveFormsModule, FormsModule, CommonModule, AppointmentRequestComponent],
    providers: [...standaloneComponentTestProviders, 
        { provide: Router, useValue: routerStub },
        { provide: ToastrService, useValue: toastrStub },
        { provide: AuthServiceService, useValue: { getloggedStatus: () => of(true) } },
        { provide: SpecializationService, useValue: { getSpecializations: () => of({ items: [] }) } },
        { provide: DoctorService, useValue: { getAllDoctors: () => of({ items: [] }) } },
        { provide: AppointmentService, useValue: jasmine.createSpyObj('AppointmentService', ['postAppointment', 'getUserAppointments', 'deleteBookingById']) },
    ],
}).compileComponents();

      fixture = TestBed.createComponent(AppointmentRequestComponent);
      fixture.detectChanges();
    });

    it('should have no critical WCAG 2.1 AA violations', async () => {
      await expectNoA11yViolations(fixture.nativeElement);
    });

    it('should expose aria labels on appointment form controls', () => {
      const root = fixture.nativeElement;
      expect(root.querySelector('[formcontrolname="name"]')?.getAttribute('aria-label')).toBe('Full name');
      expect(root.querySelector('[formcontrolname="email"]')?.getAttribute('aria-label')).toBe('Email address');
      expect(root.querySelector('[formcontrolname="phone"]')?.getAttribute('aria-label')).toBe('Phone number');
      expect(root.querySelector('[formcontrolname="date"]')?.getAttribute('aria-label')).toBe('Preferred appointment date');
    });

    it('should format appointment fee with CurrencyPipe', () => {
      expect(fixture.componentInstance.appointmentFee).toBe(30);
      fixture.detectChanges();
      const submitBtn = fixture.nativeElement.querySelector('button.btn-style-one[type="submit"]');
      expect(submitBtn?.getAttribute('data-content')).toContain('$30.00');
    });
  });

  describe('Admin dashboard page', () => {
    let fixture: ComponentFixture<BoardComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
    imports: [ReactiveFormsModule, FormsModule, CommonModule, BoardComponent, SideBarStubComponent, ChartStubComponent, DeleteModalStubComponent],
    schemas: [NO_ERRORS_SCHEMA],
    providers: [...standaloneComponentTestProviders, 
        { provide: ReloadService, useValue: reloadStub },
        { provide: ToastrService, useValue: toastrStub },
        { provide: AppointmentService, useValue: {
                getAppointments: () => of({ items: [], currentPage: 1, pageCount: 0, totalCount: 0 }),
                deleteBookingById: () => of({}),
                editeBooking: () => of({}),
            } },
        { provide: DoctorService, useValue: { getAllDoctors: () => of({ items: [], totalCount: 0 }) } },
        { provide: PatientService, useValue: { getAllPatient: () => of({ items: [], totalCount: 0, pageCount: 0, currentPage: 1, pageSize: 20 }) } },
        { provide: TotalEarningsService, useValue: { getTotalEarnings: () => of({ totalEarnings: 12500 }) } },
        { provide: AuthServiceService, useValue: jasmine.createSpyObj('AuthServiceService', ['isRole', 'getUsernameFromToken', 'getloggedStatus']) },
    ],
}).compileComponents();

      fixture = TestBed.createComponent(BoardComponent);
      fixture.detectChanges();
    });

    it('should have no critical WCAG 2.1 AA violations', async () => {
      await expectNoA11yViolations(fixture.nativeElement);
    });

    it('should format earnings with CurrencyPipe on the dashboard', () => {
      const earningsBox = fixture.componentInstance.infoBoxes.find((box) => box.isCurrency);
      expect(earningsBox?.number).toBe(12500);
      fixture.detectChanges();
      const rendered = fixture.nativeElement.textContent as string;
      expect(rendered).toContain('$12,500');
    });

    it('should use DatePipe for appointment dates in the dashboard table', () => {
      fixture.componentInstance.appointments = [{
        appointmentId: 1,
        appointmentDate: '2026-07-23T14:30:00',
        patient: { name: 'Jane Doe' },
        doctor: { name: 'Dr Smith', specializations: ['Cardiology'] },
      }];
      fixture.detectChanges();
      const tableText = fixture.nativeElement.textContent as string;
      expect(tableText).toMatch(/07\/23\/2026/);
    });
  });
});

import { Component, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { SpecializationService } from '../services/specialization.service';
import { DoctorService } from '../services/doctor.service';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AppointmentService } from '../services/appointment.service';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Subscription, combineLatest, startWith } from 'rxjs';
import { NgClass, CurrencyPipe, DatePipe } from '@angular/common';
import { PaymentComponent } from '../Payment/Payment.component';
import { DoctorAvailabilityService, MedicalCenterDoctorAvailability } from '../services/doctor-availability.service';
import { dayOfWeekName, generateTimeSlots, TimeSlot } from '../../../utils/time-slot-generator';

@Component({
    selector: 'app-appointment-request',
    templateUrl: './appointment-request.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./appointment-request.component.css'],
    imports: [ReactiveFormsModule, NgClass, PaymentComponent, CurrencyPipe, DatePipe]
})
export class AppointmentRequestComponent implements OnInit, OnDestroy {

  private subscriptions: Subscription = new Subscription(); 
  specializations: any[] = [];
  doctorsData: any[] = [];
  filteredDoctors: any[] = [];
  selectedDepartment: string = '';
  appointmentForm!: FormGroup;
  isLoggedIn = true;
  availableTimeSlots: TimeSlot[] = [];
  availabilityMessage = '';
  readonly slotDurationMinutes = 30;

  showAppointments: boolean = false;
  userAppointments: any[] = [];
  isLoading: boolean = false;
  readonly appointmentFee = 30;
  
  constructor(
    private specializationService: SpecializationService,
    private doctorService: DoctorService,
    private availabilityService: DoctorAvailabilityService,
    private fb: FormBuilder,
    private appointmentsService: AppointmentService,
    private authService: AuthServiceService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.appointmentForm = this.fb.group({
      name: ['', [Validators.required, Validators.pattern(/^[a-zA-Z\s]+$/)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^[0-9]{10,15}$/)]],
      date: ['', Validators.required],
      department: ['', Validators.required],
      doctor: ['', Validators.required],
      timeSlot: ['', Validators.required],
      message: ['', [Validators.required, Validators.minLength(10)]]
    });

    this.loadDoctors();
    this.loadSpecializations();
    this.checkAuthentication();
    this.watchDoctorAndDateChanges();
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  get name() { return this.appointmentForm.get('name'); }
  get email() { return this.appointmentForm.get('email'); }
  get phone() { return this.appointmentForm.get('phone'); }
  get date() { return this.appointmentForm.get('date'); }
  get department() { return this.appointmentForm.get('department'); }
  get doctor() { return this.appointmentForm.get('doctor'); }
  get timeSlot() { return this.appointmentForm.get('timeSlot'); }
  get message() { return this.appointmentForm.get('message'); }

  loadSpecializations() {
    const specSub = this.specializationService.getSpecializations().subscribe(
      (data) => { this.specializations = data.items; },
      () => {}
    );
    this.subscriptions.add(specSub);
  }

  loadDoctors() {
    const docSub = this.doctorService.getAllDoctors().subscribe(
      (result) => {
        if (result?.items) {
          this.doctorsData = result.items;
        }
      },
      () => {}
    );
    this.subscriptions.add(docSub);
  }

  checkAuthentication() {
    const authSub = this.authService.getloggedStatus().subscribe((status) => {
      this.isLoggedIn = status;
    });
    this.subscriptions.add(authSub);
  }

  filterDoctorsByDepartment() {
    this.appointmentForm.patchValue({ doctor: '', timeSlot: '' });
    this.availableTimeSlots = [];
    this.availabilityMessage = '';

    if (this.selectedDepartment) {
      this.filteredDoctors = this.doctorsData.filter((doctor) =>
        doctor.specializations?.some((name: string) => name === this.selectedDepartment)
      );
    } else {
      this.filteredDoctors = [];
    }
  }

  watchDoctorAndDateChanges() {
    const sub = combineLatest([
      this.appointmentForm.get('doctor')!.valueChanges.pipe(startWith('')),
      this.appointmentForm.get('date')!.valueChanges.pipe(startWith('')),
    ]).subscribe(([doctorId, dateValue]) => {
      this.appointmentForm.patchValue({ timeSlot: '' }, { emitEvent: false });
      this.loadAvailableTimeSlots(doctorId, dateValue);
    });
    this.subscriptions.add(sub);
  }

  loadAvailableTimeSlots(doctorId: string, dateValue: string) {
    this.availableTimeSlots = [];
    this.availabilityMessage = '';

    if (!doctorId || !dateValue) {
      return;
    }

    const selectedDoctor = this.doctorsData.find((doctor) => doctor.id === doctorId);
    if (!selectedDoctor) {
      this.availabilityMessage = 'No available slots for this date';
      return;
    }

    const selectedDate = new Date(`${dateValue}T00:00:00`);
    const dayName = dayOfWeekName(selectedDate);
    const medicalCenterId = selectedDoctor.medicalCenterId ?? 2;

    const availabilitySub = this.availabilityService.getAvailabilities().subscribe({
      next: (result) => {
        const availability = (result.items ?? []).find((slot: MedicalCenterDoctorAvailability) =>
          slot.medicalCenterId === medicalCenterId &&
          slot.dayOfWeek?.toLowerCase() === dayName.toLowerCase() &&
          slot.isAvailable !== false
        );

        if (!availability?.startTime || !availability.endTime) {
          this.availabilityMessage = 'No available slots for this date. Please select a different date.';
          return;
        }

        const start = this.mergeDateAndTime(selectedDate, availability.startTime);
        const end = this.mergeDateAndTime(selectedDate, availability.endTime);
        this.availableTimeSlots = generateTimeSlots(start, end, this.slotDurationMinutes);

        if (this.availableTimeSlots.length === 0) {
          this.availabilityMessage = 'No available slots for this date. Please select a different date.';
        }
      },
      error: () => {
        this.availabilityMessage = 'Unable to load availability. Please try again later.';
      }
    });
    this.subscriptions.add(availabilitySub);
  }

  selectTimeSlot(slot: TimeSlot) {
    this.appointmentForm.patchValue({ timeSlot: slot.startTime.toISOString() });
  }

  isSlotSelected(slot: TimeSlot): boolean {
    return this.timeSlot?.value === slot.startTime.toISOString();
  }

  private mergeDateAndTime(date: Date, timeValue: string): Date {
    const time = new Date(timeValue);
    const merged = new Date(date);
    merged.setHours(time.getHours(), time.getMinutes(), 0, 0);
    return merged;
  }

  postAppointment(appointmentData: any) {
    const appointmentSub = this.appointmentsService.postAppointment(appointmentData).subscribe(
      () => {
        this.toastr.success('Appointment saved!', 'Success', { positionClass: 'toast-bottom-left' });
        this.toastr.info('Please check your email account to verify', 'Success', { positionClass: 'toast-bottom-left' });
      },
      () => {
        this.toastr.info('Please fill all required fields.');
      }
    );
    this.subscriptions.add(appointmentSub);
  }

  paymentSuccessful: boolean = false;
  pendingAppointment: any = null;
  showModal = false;

  onSubmit() {
    if (this.isLoggedIn) {
      if (this.appointmentForm.valid && this.availableTimeSlots.length > 0) {
        const selectedDoctor = this.doctorsData.find((doctor) => doctor.id === this.doctor?.value);
        const slotStart = new Date(this.timeSlot?.value);
        const appointmentData = {
          name: this.name?.value,
          email: this.email?.value,
          phone: this.phone?.value,
          doctorId: this.doctor?.value,
          medicalCenterId: selectedDoctor?.medicalCenterId ?? 2,
          probableStartTime: slotStart.toISOString(),
          appointmentTakenDate: slotStart.toISOString(),
        };
        this.toastr.info(`The total cost for your appointment is ${this.appointmentFee.toLocaleString('en-US', { style: 'currency', currency: 'USD' })}. Secure your booking now!`, 'Payment Details', {
          positionClass: 'toast-bottom-left'
        });
        this.showModal = true;
        this.pendingAppointment = appointmentData;
      } else {
        this.toastr.info('Please fill all required fields and select a time slot.');
      }
    } else {
      this.toastr.warning('Please login to book an appointment');
      this.router.navigate(['/auth/login']);
    }
  }

  handlePaymentStatus(status: boolean) {
    this.paymentSuccessful = status;
    if (this.paymentSuccessful && this.pendingAppointment) {
      this.postAppointment(this.pendingAppointment);
      this.appointmentForm.reset();
      this.availableTimeSlots = [];
      this.showModal = false;
      this.pendingAppointment = null;
    }
  }

  toggleAppointmentsTable() {
    this.showAppointments = !this.showAppointments;
    if (this.showAppointments) {
      this.loadUserAppointments();
    }
  }

  loadUserAppointments() {
    if (!this.isLoggedIn) {
      this.toastr.warning('Please login to view your appointments');
      this.router.navigate(['/auth/login']);
      return;
    }

    this.isLoading = true;
    const appointmentsSub = this.appointmentsService.getUserAppointments().subscribe(
      (result) => {
        this.userAppointments = result.items;
        this.isLoading = false;
      },
      () => {
        this.toastr.error('Unable to load appointments', 'Error');
        this.isLoading = false;
      }
    );
    this.subscriptions.add(appointmentsSub);
  }

  cancelAppointment(appointmentId: number) {
      const cancelSub = this.appointmentsService.deleteBookingById(appointmentId).subscribe(
        () => {
          this.toastr.success('Appointment cancelled successfully');
          this.userAppointments = this.userAppointments.filter(app => app.id !== appointmentId);
        },
        () => {
          this.toastr.error('Failed to cancel appointment', 'Error');
        }
      );
      this.subscriptions.add(cancelSub);
  }

  getStatusBadgeClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'confirmed':
        return 'bg-success';
      case 'pending':
        return 'bg-warning';
      case 'cancelled':
        return 'bg-danger';
      case 'completed':
        return 'bg-info';
      default:
        return 'bg-secondary';
    }
  }
}

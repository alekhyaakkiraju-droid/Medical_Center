import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { PatientService } from '../../services/patient.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { Subscription } from 'rxjs';
import { AppointmentService } from '../../../pages/general/services/appointment.service';
import { MENU } from '../../menu';
import { SideBarComponent } from '../side-bar/side-bar.component';
@Component({
    selector: 'app-patients',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './patients.component.html',
    imports: [SideBarComponent]
})
export class PatientsComponent implements OnInit , OnDestroy{

  patientData: any[] = [];
  menuItems = MENU;
  isLoading: boolean = true;
  numOfAppointments: number = 0;
  errorMessage: string = '';
  patientSubscription !: Subscription;

  constructor(private patientService: PatientService , private reload :ReloadService ,
    private appointmentService : AppointmentService
  ) {}
  ngOnDestroy(): void {
   if(this.patientService){
      this.patientSubscription.unsubscribe();
   }
  }
  ngAfterViewInit(): void {
    this.reload.initializeLoader();
  }
  ngOnInit(): void {
    this.fetchPatientReviews();
     this.setBadgeForAppointments();
    this.loadAppointments();
  }
  setBadgeForAppointments() {
    const appointmentItem = this.menuItems.find(item => item.title === 'Appointment');
    if (appointmentItem) {
      appointmentItem.badge = this.numOfAppointments.toString();
    }
  }

  fetchPatientReviews(): void {
   this.patientSubscription= this.patientService.getAllPatient().subscribe({
      next: (data) => {
        this.patientData = data.items ?? [];
      },
      error: (error) => {
        this.errorMessage = 'Failed to fetch patient reviews.';
      }
    });
  }
  loadAppointments(): void {
    const appointmentSub = this.appointmentService.getAppointments().subscribe(
      (data) => {
        this.numOfAppointments = data['totalCount'];
        this.setBadgeForAppointments();
      },
      (error) => {
      }
    );
  }
}

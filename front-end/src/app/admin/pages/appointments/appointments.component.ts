import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { AppointmentService } from '../../../pages/general/services/appointment.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { Subscription } from 'rxjs';
import { MENU } from '../../menu';
import { SideBarComponent } from '../side-bar/side-bar.component';
import { TempAppointmentComponent } from '../temp-appointment/temp-appointment.component';

@Component({
    selector: 'app-appointments',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './appointments.component.html',
    imports: [SideBarComponent, TempAppointmentComponent]
})
export class AppointmentsComponent implements OnInit, OnDestroy {
  appointments: any[] = [];
  numOfAppointments: number = 0;
  menuItems = MENU;
  appointmentsSubscription!: Subscription;
  constructor(private appointmentService: AppointmentService, private reload: ReloadService) { }
  ngOnDestroy(): void {
    if (this.appointmentsSubscription) {
      this.appointmentsSubscription.unsubscribe();
    }
  }

  ngAfterViewInit(): void {
    this.reload.initializeLoader();
  }

  ngOnInit(): void {
    this.loadAppointments();
  }

  loadAppointments(): void {
    this.appointmentsSubscription = this.appointmentService.getAppointments().subscribe(
      (data) => {
        this.appointments = data.items;
        this.numOfAppointments = data.totalCount;
        console.log('Fetched appointments:', this.appointments);
      },
      (error) => {
        console.error('Error fetching appointments:', error);
      }
    );
  }

  setBadgeForAppointments() {
    const appointmentItem = this.menuItems.find(item => item.title === 'Appointment');
    if (appointmentItem) {
      appointmentItem.badge = this.numOfAppointments.toString();
      console.log('Appointment badge set to:', appointmentItem.badge);
    }
  }


}

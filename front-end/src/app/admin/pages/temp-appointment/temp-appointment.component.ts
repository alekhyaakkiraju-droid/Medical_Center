import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { AppointmentService } from '../../../pages/general/services/appointment.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { MENU } from '../../menu';
import { DatePipe } from '@angular/common';

@Component({
    selector: 'app-temp-appointment',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './temp-appointment.component.html',
    imports: [DatePipe]
})
export class TempAppointmentComponent implements OnInit {

  appointments: any[] = [];
  numOfAppointments: number = 0;
  constructor(private appointmentService: AppointmentService, private reload: ReloadService) { }

  ngOnInit(): void {
    this.loadAppointments();
  }

  loadAppointments(): void {
    this.appointmentService.getAppointments().subscribe(
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

}

import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { SideBarComponent } from '../../../admin/pages/side-bar/side-bar.component';
import { AppointmentService } from '../../../pages/general/services/appointment.service';
import { Appointment } from '../../../pages/models';

@Component({
  selector: 'app-patient-appointments',
  changeDetection: ChangeDetectionStrategy.Eager,
  template: `
    <app-side-bar></app-side-bar>
    <main class="p-4 sm:ml-64">
      <h1>My Appointments</h1>
      @if (appointments.length === 0) {
        <p>No appointments found.</p>
      } @else {
        <ul>
          @for (a of appointments; track a.appointmentId) {
            <li>{{ a.doctor?.name }} — {{ a.appointmentDate | date:'medium' }} ({{ a.appointmentStatus }})</li>
          }
        </ul>
      }
    </main>
  `,
  imports: [SideBarComponent, DatePipe],
})
export class PatientAppointmentsComponent implements OnInit {
  appointments: Appointment[] = [];
  constructor(private appointmentService: AppointmentService) {}
  ngOnInit(): void {
    this.appointmentService.getUserAppointments(1, 100).subscribe((r) => {
      this.appointments = r.items ?? [];
    });
  }
}

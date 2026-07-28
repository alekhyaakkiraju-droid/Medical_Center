import { Component, OnDestroy, OnInit, AfterViewInit, ChangeDetectionStrategy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { SideBarComponent } from '../../../admin/pages/side-bar/side-bar.component';
import { AuthServiceService } from '../../../pages/auth/auth-services/auth-service.service';
import { AppointmentService } from '../../../pages/general/services/appointment.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { Appointment } from '../../../pages/models';

@Component({
  selector: 'app-patient-home',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './patient-home.component.html',
  styleUrls: ['./patient-home.component.css'],
  imports: [SideBarComponent, DatePipe, RouterLink],
})
export class PatientHomeComponent implements OnInit, AfterViewInit, OnDestroy {
  welcomeName = 'Patient';
  upcomingAppointments: Appointment[] = [];
  recentVisits: Appointment[] = [];
  loadError = false;
  private subscriptions: Subscription[] = [];

  constructor(
    private authService: AuthServiceService,
    private appointmentService: AppointmentService,
    private reload: ReloadService
  ) {}

  ngOnInit(): void {
    this.welcomeName = this.authService.getUserName() ?? 'Patient';
    this.loadAppointments();
  }

  ngAfterViewInit(): void { this.reload.initializeLoader(); }
  ngOnDestroy(): void { this.subscriptions.forEach((s) => s.unsubscribe()); }

  loadAppointments(): void {
    const sub = this.appointmentService.getUserAppointments(1, 100).subscribe({
      next: (result) => {
        const items = result.items ?? [];
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        this.upcomingAppointments = items.filter((a) => this.isUpcoming(a, today));
        this.recentVisits = items
          .filter((a) => ['Completed', 'Cancelled'].includes(a.appointmentStatus ?? ''))
          .sort((a, b) => new Date(b.appointmentDate ?? 0).getTime() - new Date(a.appointmentDate ?? 0).getTime())
          .slice(0, 5);
        this.loadError = false;
      },
      error: () => { this.loadError = true; },
    });
    this.subscriptions.push(sub);
  }

  private isUpcoming(appointment: Appointment, today: Date): boolean {
    if (appointment.appointmentStatus !== 'Active') {
      return false;
    }
    const date = appointment.appointmentDate ? new Date(appointment.appointmentDate) : null;
    return date ? date >= today : false;
  }
}

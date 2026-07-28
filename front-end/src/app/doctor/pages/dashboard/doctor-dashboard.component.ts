import { Component, OnDestroy, OnInit, AfterViewInit, ChangeDetectionStrategy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subscription, forkJoin } from 'rxjs';
import { SideBarComponent } from '../../../admin/pages/side-bar/side-bar.component';
import { AuthServiceService } from '../../../pages/auth/auth-services/auth-service.service';
import { DoctorAppointmentsService } from '../../services/doctor-appointments.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { Booking } from '../../../pages/models';

@Component({
  selector: 'app-doctor-dashboard',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './doctor-dashboard.component.html',
  styleUrls: ['./doctor-dashboard.component.css'],
  imports: [SideBarComponent, DatePipe, RouterLink],
})
export class DoctorDashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  doctorId = '';
  todaysBookings: Booking[] = [];
  upcomingCount = 0;
  loadError = false;
  isLoading = true;
  welcomeName = 'Doctor';
  readonly today = new Date();
  private subscriptions: Subscription[] = [];

  constructor(
    private authService: AuthServiceService,
    private doctorAppointmentsService: DoctorAppointmentsService,
    private reload: ReloadService
  ) {}

  ngOnInit(): void {
    this.welcomeName = this.authService.getUserName() ?? this.authService.getUsernameFromToken() ?? 'Doctor';
    const doctorId = this.authService.getNameIdentifier();
    if (!doctorId) { this.loadError = true; this.isLoading = false; return; }
    this.doctorId = doctorId;
    this.loadDashboardData();
  }

  ngAfterViewInit(): void { this.reload.initializeLoader(); }
  ngOnDestroy(): void { this.subscriptions.forEach((sub) => sub.unsubscribe()); }

  loadDashboardData(): void {
    this.isLoading = true;
    this.loadError = false;
    const sub = forkJoin({
      today: this.doctorAppointmentsService.getTodaysBookings(this.doctorId),
      upcoming: this.doctorAppointmentsService.getUpcomingBookings(this.doctorId),
    }).subscribe({
      next: ({ today, upcoming }) => {
        this.todaysBookings = today.items ?? [];
        this.upcomingCount = upcoming.totalCount ?? upcoming.items?.length ?? 0;
        this.isLoading = false;
      },
      error: () => { this.loadError = true; this.isLoading = false; },
    });
    this.subscriptions.push(sub);
  }

  retryLoad(): void { this.loadDashboardData(); }
}

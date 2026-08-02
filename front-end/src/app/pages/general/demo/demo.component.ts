import { Component, ChangeDetectionStrategy } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { NgClass } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../../environments/environment';
import { getRoleBasedRedirectUrl } from '../../../core/utils/role-redirect.util';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';

export interface DemoRole {
  id: string;
  title: string;
  subtitle: string;
  email: string;
  destination: string;
  iconClass: string;
  accentClass: string;
  highlights: string[];
}

@Component({
  selector: 'app-demo',
  templateUrl: './demo.component.html',
  styleUrls: ['./demo.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, NgClass],
})
export class DemoComponent {
  readonly demoPassword = environment.demoPassword || 'UatSeed123!';
  readonly showDemoPassword = true;
  loadingRole: string | null = null;

  readonly roles: DemoRole[] = [
    {
      id: 'admin',
      title: 'Administrator',
      subtitle: 'Operations dashboard, analytics, and appointment management',
      email: 'admin@uat.careshift.local',
      destination: '/admin/dashboard',
      iconClass: 'fa-solid fa-shield-halved',
      accentClass: 'demo-card--admin',
      highlights: ['Live KPI cards', 'Export PDF / Excel', 'Edit & cancel bookings'],
    },
    {
      id: 'doctor',
      title: 'Doctor',
      subtitle: 'Clinical schedule and patient appointments',
      email: 'dr.smith@uat.careshift.local',
      destination: '/doctor/dashboard',
      iconClass: 'fa-solid fa-user-doctor',
      accentClass: 'demo-card--doctor',
      highlights: ['Today & upcoming filters', 'Assigned patients', 'Profile & reviews'],
    },
    {
      id: 'patient',
      title: 'Patient',
      subtitle: 'Book care and manage your visits',
      email: 'patient.alice@uat.careshift.local',
      destination: '/patient/home',
      iconClass: 'fa-solid fa-heart-pulse',
      accentClass: 'demo-card--patient',
      highlights: ['My Appointments list', 'Book with real doctors', 'Secure login flow'],
    },
  ];

  readonly walkthrough = [
    { step: 1, title: 'Explore the public site', detail: 'Browse services, team, and gallery — then open Live Demo.' },
    { step: 2, title: 'Enter as Patient', detail: 'Book or view seeded appointments — you land on My Appointments.' },
    { step: 3, title: 'Switch to Doctor', detail: 'Sign out, return here, enter as Dr. Smith — see today\'s schedule.' },
    { step: 4, title: 'Finish as Admin', detail: 'Dashboard shows real counts, charts, and the full appointment table.' },
  ];

  constructor(
    private authService: AuthServiceService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  enterAs(role: DemoRole): void {
    if (this.loadingRole) {
      return;
    }

    const password = this.demoPassword || 'UatSeed123!';
    if (!password) {
      this.toastr.error('Demo password is not configured for this environment.');
      return;
    }

    this.loadingRole = role.id;
    this.authService.login(role.email, password).subscribe({
      next: () => {
        this.toastr.success(`Welcome — exploring as ${role.title}`);
        this.router.navigate([getRoleBasedRedirectUrl([role.id === 'patient' ? 'user' : role.id])]);
        this.loadingRole = null;
      },
      error: () => {
        this.toastr.error('Demo login failed. Is the API running on port 8090?');
        this.loadingRole = null;
      },
    });
  }

  copyPassword(): void {
    if (!this.demoPassword) {
      return;
    }
    navigator.clipboard?.writeText(this.demoPassword);
    this.toastr.info('Demo password copied');
  }
}

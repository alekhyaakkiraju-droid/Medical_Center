import { Routes } from '@angular/router';
import { RelatedAppointmentsComponent } from './pages/related-appointments/related-appointments.component';
import { DoctorProfileComponent } from './pages/doctor-profile/doctor-profile.component';
import { PatientReviewsComponent } from './pages/patient-reviews/patient-reviews.component';
import { DoctorDashboardComponent } from './pages/dashboard/doctor-dashboard.component';

export const DOCTOR_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DoctorDashboardComponent, data: { title: 'Doctor Dashboard - CareShift' } },
  { path: 'doctor-appointments', component: RelatedAppointmentsComponent, data: { title: 'Appointments - CareShift Doctor' } },
  { path: 'doctor-profile', component: DoctorProfileComponent, data: { title: 'Profile - CareShift Doctor' } },
  { path: 'patient-reviews', component: PatientReviewsComponent, data: { title: 'Patient Reviews - CareShift Doctor' } },
];

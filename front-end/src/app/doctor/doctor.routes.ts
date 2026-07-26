import { Routes } from '@angular/router';
import { RelatedAppointmentsComponent } from './pages/related-appointments/related-appointments.component';
import { DoctorProfileComponent } from './pages/doctor-profile/doctor-profile.component';
import { PatientReviewsComponent } from './pages/patient-reviews/patient-reviews.component';

export const DOCTOR_ROUTES: Routes = [
  { path: 'doctor-appointments', component: RelatedAppointmentsComponent },
  { path: 'doctor-profile', component: DoctorProfileComponent },
  { path: 'patient-reviews', component: PatientReviewsComponent },
];

import { Routes } from '@angular/router';
import { PatientHomeComponent } from './pages/home/patient-home.component';
import { PatientAppointmentsComponent } from './pages/appointments/patient-appointments.component';

export const PATIENT_ROUTES: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: PatientHomeComponent },
  { path: 'appointments', component: PatientAppointmentsComponent },
];

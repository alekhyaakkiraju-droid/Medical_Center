import { Routes } from '@angular/router';
import { DoctorsComponent } from './pages/doctors/doctors.component';
import { BoardComponent } from './pages/board/board.component';
import { PatientsComponent } from './pages/patients/patients.component';
import { AppointmentsComponent } from './pages/appointments/appointments.component';
import { ChartComponent } from './pages/chart/chart.component';

export const ADMIN_ROUTES: Routes = [
  { path: 'doctors', component: DoctorsComponent },
  { path: 'dashboard', component: BoardComponent },
  { path: 'chart', component: ChartComponent },
  { path: 'patients', component: PatientsComponent },
  { path: 'appointments', component: AppointmentsComponent },
];

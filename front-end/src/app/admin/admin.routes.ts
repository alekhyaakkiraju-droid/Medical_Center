import { Routes } from '@angular/router';
import { DoctorsComponent } from './pages/doctors/doctors.component';
import { BoardComponent } from './pages/board/board.component';
import { PatientsComponent } from './pages/patients/patients.component';
import { AppointmentsComponent } from './pages/appointments/appointments.component';
import { ChartComponent } from './pages/chart/chart.component';

export const ADMIN_ROUTES: Routes = [
  { path: 'doctors', component: DoctorsComponent, data: { title: 'Doctors - CareShift Admin' } },
  { path: 'dashboard', component: BoardComponent, data: { title: 'Dashboard - CareShift Admin' } },
  { path: 'chart', component: ChartComponent, data: { title: 'Charts - CareShift Admin' } },
  { path: 'patients', component: PatientsComponent, data: { title: 'Patients - CareShift Admin' } },
  { path: 'appointments', component: AppointmentsComponent, data: { title: 'Appointments - CareShift Admin' } },
];

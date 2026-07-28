import { Routes } from '@angular/router';
import { HomeComponent } from './pages/general/Home/Home.component';
import { DemoComponent } from './pages/general/demo/demo.component';
import { ErrorPageComponent } from './pages/general/errorPage/errorPage.component';
import { DoctorGuard } from './doctor/guard/doctor.guard';
import { AdminGuard } from './admin/guard/admin.guard';
import { PatientGuard } from './core/guards/patient.guard';
import { AuthGuard } from './pages/auth/guard/auth.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full', data: { title: 'Home - CareShift' } },
  { path: 'demo', component: DemoComponent, data: { title: 'Demo - CareShift' } },
  { path: 'home', component: HomeComponent, canActivate: [AuthGuard], data: { title: 'Home - CareShift' } },
  { path: 'admin', canActivate: [AdminGuard], loadChildren: () => import('./admin/admin.routes').then((m) => m.ADMIN_ROUTES), data: { title: 'Admin - CareShift' } },
  { path: 'doctor', canActivate: [DoctorGuard], loadChildren: () => import('./doctor/doctor.routes').then((m) => m.DOCTOR_ROUTES), data: { title: 'Doctor Portal - CareShift' } },
  { path: 'patient', canActivate: [PatientGuard], loadChildren: () => import('./patient/patient.routes').then((m) => m.PATIENT_ROUTES), data: { title: 'Patient Portal - CareShift' } },
  { path: 'pages', loadChildren: () => import('./pages/general/general.routes').then((m) => m.GENERAL_ROUTES) },
  { path: 'auth', loadChildren: () => import('./pages/auth/auth.routes').then((m) => m.AUTH_ROUTES) },
  { path: 'error', component: ErrorPageComponent, data: { type: 404, title: 'Page Not Found - CareShift', desc: "Oopps!! The page you were looking for doesn't exist." } },
  { path: 'error/:type', component: ErrorPageComponent, data: { title: 'Error - CareShift' } },
  { path: '**', redirectTo: 'error', pathMatch: 'full' },
];

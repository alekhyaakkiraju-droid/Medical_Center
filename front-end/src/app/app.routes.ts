import { Routes } from '@angular/router';
import { ErrorPageComponent } from './pages/general/errorPage/errorPage.component';
import { DoctorGuard } from './doctor/guard/doctor.guard';
import { AdminGuard } from './admin/guard/admin.guard';
import { PatientGuard } from './core/guards/patient.guard';
import { AuthGuard } from './pages/auth/guard/auth.guard';
import { nppGuard } from './core/guards/npp.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./pages/general/Home/Home.component').then((m) => m.HomeComponent), pathMatch: 'full', data: { title: 'Home - CareShift' } },
  { path: 'demo', redirectTo: 'auth/login', pathMatch: 'full' },
  { path: 'home', loadComponent: () => import('./pages/general/Home/Home.component').then((m) => m.HomeComponent), canActivate: [AuthGuard, nppGuard], data: { title: 'Home - CareShift' } },
  { path: 'admin', canActivate: [AdminGuard, nppGuard], loadChildren: () => import('./admin/admin.routes').then((m) => m.ADMIN_ROUTES), data: { title: 'Admin - CareShift' } },
  { path: 'doctor', canActivate: [DoctorGuard, nppGuard], loadChildren: () => import('./doctor/doctor.routes').then((m) => m.DOCTOR_ROUTES), data: { title: 'Doctor Portal - CareShift' } },
  { path: 'patient', canActivate: [PatientGuard, nppGuard], loadChildren: () => import('./patient/patient.routes').then((m) => m.PATIENT_ROUTES), data: { title: 'Patient Portal - CareShift' } },
  { path: 'pages', loadChildren: () => import('./pages/general/general.routes').then((m) => m.GENERAL_ROUTES) },
  { path: 'auth', loadChildren: () => import('./pages/auth/auth.routes').then((m) => m.AUTH_ROUTES) },
  { path: 'error', component: ErrorPageComponent, data: { type: 404, title: 'Page Not Found - CareShift', desc: "Oopps!! The page you were looking for doesn't exist." } },
  { path: 'error/:type', component: ErrorPageComponent, data: { title: 'Error - CareShift' } },
  { path: '**', redirectTo: 'error', pathMatch: 'full' },
];

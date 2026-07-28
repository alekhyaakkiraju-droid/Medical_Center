import { Routes } from '@angular/router';
import { HomeComponent } from './pages/general/Home/Home.component';
import { DemoComponent } from './pages/general/demo/demo.component';
import { ErrorPageComponent } from './pages/general/errorPage/errorPage.component';
import { DoctorGuard } from './doctor/guard/doctor.guard';
import { AdminGuard } from './admin/guard/admin.guard';
import { PatientGuard } from './core/guards/patient.guard';
import { AuthGuard } from './pages/auth/guard/auth.guard';
import { nppGuard } from './core/guards/npp.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'demo', component: DemoComponent },
  { path: 'home', component: HomeComponent, canActivate: [AuthGuard, nppGuard] },
  { path: 'admin', canActivate: [AdminGuard, nppGuard], loadChildren: () => import('./admin/admin.routes').then((m) => m.ADMIN_ROUTES) },
  { path: 'doctor', canActivate: [DoctorGuard, nppGuard], loadChildren: () => import('./doctor/doctor.routes').then((m) => m.DOCTOR_ROUTES) },
  { path: 'patient', canActivate: [PatientGuard, nppGuard], loadChildren: () => import('./patient/patient.routes').then((m) => m.PATIENT_ROUTES) },
  { path: 'pages', loadChildren: () => import('./pages/general/general.routes').then((m) => m.GENERAL_ROUTES) },
  { path: 'auth', loadChildren: () => import('./pages/auth/auth.routes').then((m) => m.AUTH_ROUTES) },
  { path: 'error', component: ErrorPageComponent, data: { type: 404, title: 'Page Not Found', desc: "Oopps!! The page you were looking for doesn't exist." } },
  { path: 'error/:type', component: ErrorPageComponent },
  { path: '**', redirectTo: 'error', pathMatch: 'full' },
];

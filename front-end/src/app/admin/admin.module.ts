import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminComponent } from './admin.component';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { DoctorsComponent } from './pages/doctors/doctors.component';
import { BoardComponent } from './pages/board/board.component';
import { PatientsComponent } from './pages/patients/patients.component';
import { AppointmentsComponent } from './pages/appointments/appointments.component';
import { TempAppointmentComponent } from './pages/temp-appointment/temp-appointment.component';
import { ChartComponent } from './pages/chart/chart.component';
import { SharedModule } from '../shared/shared.module';

const routes: Routes = [
  { path: 'doctors', component: DoctorsComponent },
  { path: 'dashboard', component: BoardComponent },
  { path: 'chart', component: ChartComponent },
  { path: 'patients', component: PatientsComponent },
  { path: 'appointments', component: AppointmentsComponent }
]
@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routes),
        ReactiveFormsModule,
        RouterModule,
        FormsModule,
        SharedModule,
        DoctorsComponent,
        BoardComponent,
        ChartComponent,
        PatientsComponent,
        AppointmentsComponent,
        TempAppointmentComponent,
        AdminComponent,
    ],
    bootstrap: [AdminComponent]
})
export class AdminModule { }

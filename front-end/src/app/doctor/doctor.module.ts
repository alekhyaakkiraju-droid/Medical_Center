import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DoctorComponent } from './doctor.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { RelatedAppointmentsComponent } from './pages/related-appointments/related-appointments.component';
import { DoctorProfileComponent } from './pages/doctor-profile/doctor-profile.component';
import { PatientReviewsComponent } from './pages/patient-reviews/patient-reviews.component';
import { SharedModule } from '../shared/shared.module';

const routes: Routes = [
  { path: 'doctor-appointments', component: RelatedAppointmentsComponent },
  { path: 'doctor-profile', component: DoctorProfileComponent },
  { path: 'patient-reviews', component: PatientReviewsComponent },
]

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routes),
        ReactiveFormsModule,
        RouterModule,
        FormsModule,
        SharedModule,
        DoctorComponent,
        RelatedAppointmentsComponent,
        DoctorProfileComponent,
        PatientReviewsComponent,
    ]
})
export class DoctorModule { }

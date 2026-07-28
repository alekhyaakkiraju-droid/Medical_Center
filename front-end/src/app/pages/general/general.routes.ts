import { Routes } from '@angular/router';
import { HomeComponent } from './Home/Home.component';
import { AboutUsComponent } from './about-us/about-us.component';
import { ContactUsComponent } from './contact-us/contact-us.component';
import { BlogComponent } from './blog/blog.component';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { RequestAppointmentComponent } from './request-appointment/request-appointment.component';
import { MedicalServiceComponent } from './medical-service/medical-service.component';
import { GalleryComponent } from './gallery/gallery.component';
import { TeamComponent } from './team/team.component';
import { PaymentComponent } from './Payment/Payment.component';

export const GENERAL_ROUTES: Routes = [
  { path: 'about-us', component: AboutUsComponent, data: { title: 'About Us - CareShift' } },
  { path: 'contact', component: ContactUsComponent, data: { title: 'Contact - CareShift' } },
  { path: 'home', component: HomeComponent, data: { title: 'Home - CareShift' } },
  { path: 'blog', component: BlogComponent, data: { title: 'Blog - CareShift' } },
  { path: 'profile', component: UserProfileComponent, data: { title: 'Profile - CareShift' } },
  { path: 'appointment', component: RequestAppointmentComponent, data: { title: 'Appointment - CareShift' } },
  { path: 'service', component: MedicalServiceComponent, data: { title: 'Services - CareShift' } },
  { path: 'gallery', component: GalleryComponent, data: { title: 'Gallery - CareShift' } },
  { path: 'team', component: TeamComponent, data: { title: 'Team - CareShift' } },
  { path: 'payment', component: PaymentComponent, data: { title: 'Payment - CareShift' } },
];

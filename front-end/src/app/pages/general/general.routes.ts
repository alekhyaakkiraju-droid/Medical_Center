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
import { AuthGuard } from '../auth/guard/auth.guard';
import { nppGuard } from '../../core/guards/npp.guard';

export const GENERAL_ROUTES: Routes = [
  { path: 'about-us', component: AboutUsComponent },
  { path: 'contact', component: ContactUsComponent },
  { path: 'home', component: HomeComponent },
  { path: 'blog', component: BlogComponent },
  { path: 'profile', component: UserProfileComponent, canActivate: [AuthGuard, nppGuard] },
  { path: 'appointment', component: RequestAppointmentComponent, canActivate: [AuthGuard, nppGuard] },
  { path: 'service', component: MedicalServiceComponent },
  { path: 'gallery', component: GalleryComponent },
  { path: 'team', component: TeamComponent },
  { path: 'payment', component: PaymentComponent, canActivate: [AuthGuard, nppGuard] },
];

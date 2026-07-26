import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { LogoutComponent } from './logout/logout.component';
import { ForgetPasswordComponent } from './forgetPassword/forgetPassword.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { LoginSuccessComponent } from './LoginSuccess/LoginSuccess.component';
import { ConfirmEmailComponent } from './confirm-email/confirm-email.component';

export const AUTH_ROUTES: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'logout', component: LogoutComponent },
  { path: 'forgot-password', component: ForgetPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
  { path: 'reset-password/:token/:email', component: ForgetPasswordComponent },
  { path: 'login-success', component: LoginSuccessComponent },
  { path: 'confirm-email', component: ConfirmEmailComponent },
];

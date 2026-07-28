import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { LogoutComponent } from './logout/logout.component';
import { ForgetPasswordComponent } from './forgetPassword/forgetPassword.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { LoginSuccessComponent } from './LoginSuccess/LoginSuccess.component';
import { ConfirmEmailComponent } from './confirm-email/confirm-email.component';

export const AUTH_ROUTES: Routes = [
  { path: 'login', component: LoginComponent, data: { title: 'Login - CareShift' } },
  { path: 'register', component: RegisterComponent, data: { title: 'Register - CareShift' } },
  { path: 'logout', component: LogoutComponent, data: { title: 'Logout - CareShift' } },
  { path: 'forgot-password', component: ForgetPasswordComponent, data: { title: 'Forgot Password - CareShift' } },
  { path: 'reset-password', component: ResetPasswordComponent, data: { title: 'Reset Password - CareShift' } },
  { path: 'reset-password/:token/:email', component: ForgetPasswordComponent, data: { title: 'Reset Password - CareShift' } },
  { path: 'login-success', component: LoginSuccessComponent, data: { title: 'Login Success - CareShift' } },
  { path: 'confirm-email', component: ConfirmEmailComponent, data: { title: 'Confirm Email - CareShift' } },
];

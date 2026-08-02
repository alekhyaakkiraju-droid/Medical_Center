import { HttpErrorResponse } from '@angular/common/http';
import { AfterViewInit, Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { SnakebarService } from '../../../shared/service/SnakebarService.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { AuthServiceService } from '../auth-services/auth-service.service';
import { ForgotServiceService } from '../auth-services/forgot-service.service';
import { BehaviorSubject, Subscription } from 'rxjs';
import { ResetPasswordService } from '../auth-services/resetPassword.service';
import { ModelService } from '../auth-services/model.service';
import { ToastrService } from 'ngx-toastr';
import { ForgetPasswordComponent } from '../forgetPassword/forgetPassword.component';
import { getRoleBasedRedirectUrl } from '../../../core/utils/role-redirect.util';
import { AssetUrlPipe } from '../../../shared/asset-url.pipe';

@Component({
    selector: 'app-login',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss'],
    imports: [ReactiveFormsModule, ForgetPasswordComponent, RouterLink, AssetUrlPipe]
})
export class LoginComponent implements OnInit, AfterViewInit{

  private subscriptions: Subscription[] = [];
  loginForm: FormGroup;

  constructor(private fb: FormBuilder,
    private toastr: ToastrService,
    private reload : ReloadService,
    private authService: AuthServiceService,
    private router: Router,
    private route: ActivatedRoute,
    private forgetpasswordService :ForgotServiceService, 
    private resetPasswordService :ResetPasswordService,
    private modalService: ModelService
    ) {

    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });

    this.forgetForm = this.fb.group({
      emailForgot: ['', [Validators.required, Validators.email]],     
    });

    this.resetForm = this.fb.group({
      resetPassword: ['', [Validators.required]],     
    });
    
  }

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      if (params['reason'] === 'session-expired') {
        this.toastr.info(
          'Your session has expired due to inactivity. Please log in again.',
          'Session Expired'
        );
      }
    });
  }
  ngAfterViewInit(): void {   
    this.reload.initializeLoader();
  }
  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
  isDialogOpen = false;
  isDialogMounted = false;

  openDialog(): void {
    this.isDialogOpen = true;
    setTimeout(() => {
      this.isDialogMounted = true;
    }, 10);
  }

  closeDialog(): void {
    this.isDialogMounted = false;
    setTimeout(() => {
      this.isDialogOpen = false;
    }, 200);
  }

  confirm(): void {
    // Handle confirm logic here
    this.closeDialog();
  }

  get email() {
    return this.loginForm.get('email');
  }
  get password() {
    return this.loginForm.get('password');
  }
  getEmailErrorMessage() {
    if (this.email?.hasError('required')) return 'Email is required.';
    //if (this.email?.hasError('email')) return 'Invalid email format.';
    return '';
  }

  getPasswordErrorMessage() {
    if (this.password?.hasError('required')) return 'Password is required.';
    if (this.password?.hasError('minlength')) return 'Password must be at least 6 characters.';
    return '';
  }

  onLoginSuccess() {
    this.toastr.success(`Welcome ${this.authService.getUsernameFromToken()?.toUpperCase()}`);
  }
  onLoginFailed() {
    this.toastr.error(`Login Failed`);
  }

  errorMessage: string | null = null;
  
  //-----------------------------Login-----------------------------------
  onSubmit() {
    const { email, password } = this.loginForm.value;
    if (this.loginForm.valid) {
      const loginSub = this.authService.login(email, password).subscribe(
        () => {
          this.onLoginSuccess();
          this.navigateAfterLogin();
        },
        (error: HttpErrorResponse) => {
          if (error.status === 400) {
            this.errorMessage = 'Unable to sign in right now. Refresh the page and try again.';
          } else if (error.status === 401) {
            this.errorMessage = 'Invalid email or password. Please try again.';
          } else {
            this.errorMessage = 'Sign in failed. Check your connection and try again.';
          }
          this.onLoginFailed();
        }
      );
      this.subscriptions.push(loginSub);
    }
  }

  loginWithGoogle(): void {
    window.location.href = this.authService.googleloginUrl;
  }

  navigateAfterLogin(): void {
    const roles = this.authService.getCurrentUserRoles();
    this.router.navigate([getRoleBasedRedirectUrl(roles)]);
  }

// ------------------------------Forget password-------------------------------------
openForgetPasswordModal() {
  this.modalService.openDialog();
 /// this.openDialog();
}



forgetForm !: FormGroup;
get Forgotemail() {
  return this.forgetForm.get('emailForgot');
}
onForgotSubmit() {
  const emailForgetVal = this.forgetForm.value.emailForgot;
  const forgetSub = this.forgetpasswordService.forgetPassword(emailForgetVal).subscribe({
    next: (res) => {
      this.toastr.success(`Success: ${res.message}`);   
    },
    error: (err) =>  this.toastr.error(`Error: ${err.message}`)
  });
  this.subscriptions.push(forgetSub);
}

// -------------------------------------------------------Reset password---------

resetForm !: FormGroup;
get resetPass() {
  return this.forgetForm.get('resetPassword');
}


}

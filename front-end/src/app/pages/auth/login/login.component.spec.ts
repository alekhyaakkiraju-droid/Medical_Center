import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { LoginComponent } from './login.component';
import { AuthServiceService } from '../auth-services/auth-service.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { ToastrService } from 'ngx-toastr';
import { ForgotServiceService } from '../auth-services/forgot-service.service';
import { ResetPasswordService } from '../auth-services/resetPassword.service';
import { ModelService } from '../auth-services/model.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';
import { mockRoleTokens } from '../../../testing/fixtures/role-tokens.mock';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: {
    login: jasmine.Spy;
    getUsernameFromToken: jasmine.Spy;
    getCurrentUserRoles: jasmine.Spy;
  };
  let router: Router;

  beforeEach(async () => {
    authService = {
      login: jasmine.createSpy('login'),
      getUsernameFromToken: jasmine.createSpy('getUsernameFromToken').and.returnValue('user'),
      getCurrentUserRoles: jasmine.createSpy('getCurrentUserRoles').and.returnValue([]),
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        ...standaloneComponentTestProviders,
        { provide: AuthServiceService, useValue: authService },
        { provide: ReloadService, useValue: { initializeLoader: () => undefined } },
        { provide: ToastrService, useValue: { success: () => undefined, error: () => undefined } },
        { provide: ForgotServiceService, useValue: {} },
        { provide: ResetPasswordService, useValue: {} },
        { provide: ModelService, useValue: { openDialog: () => undefined } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    spyOn(router, 'navigate');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not suppress focus styles on interactive login controls', () => {
    const buttons = fixture.nativeElement.querySelectorAll('button');
    buttons.forEach((button: HTMLButtonElement) => {
      expect(button.className).not.toContain('outline-none');
      expect(button.className).toContain('focus-visible:ring-2');
    });
  });

  it('redirects admin users after login', () => {
    authService.getCurrentUserRoles.and.returnValue(mockRoleTokens.admin.roles);
    component.navigateAfterLogin();
    expect(router.navigate).toHaveBeenCalledWith(['/admin/dashboard']);
  });

  it('redirects doctor users after login', () => {
    authService.getCurrentUserRoles.and.returnValue(mockRoleTokens.doctor.roles);
    component.navigateAfterLogin();
    expect(router.navigate).toHaveBeenCalledWith(['/doctor/dashboard']);
  });

  it('redirects patient users after login', () => {
    authService.getCurrentUserRoles.and.returnValue(mockRoleTokens.patient.roles);
    component.navigateAfterLogin();
    expect(router.navigate).toHaveBeenCalledWith(['/patient/home']);
  });

  it('redirects unknown roles to public home', () => {
    authService.getCurrentUserRoles.and.returnValue([]);
    component.navigateAfterLogin();
    expect(router.navigate).toHaveBeenCalledWith(['/pages/home']);
  });
});

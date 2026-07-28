import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { of, throwError } from 'rxjs';
import { ContactUsComponent } from './contact-us.component';
import { ContactService } from '../services/contact.service';
import { RecaptchaService } from '../services/recaptcha.service';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('ContactUsComponent', () => {
  let component: ContactUsComponent;
  let fixture: ComponentFixture<ContactUsComponent>;
  let contactService: jasmine.SpyObj<ContactService>;
  let recaptchaService: jasmine.SpyObj<RecaptchaService>;
  let toastr: jasmine.SpyObj<ToastrService>;
  let authService: jasmine.SpyObj<AuthServiceService>;

  beforeEach(waitForAsync(() => {
    contactService = jasmine.createSpyObj('ContactService', ['submitInquiry']);
    recaptchaService = jasmine.createSpyObj('RecaptchaService', ['execute']);
    toastr = jasmine.createSpyObj('ToastrService', ['success', 'error']);
    authService = jasmine.createSpyObj('AuthServiceService', ['ensureCsrfToken']);
    authService.ensureCsrfToken.and.returnValue(of(void 0));
    recaptchaService.execute.and.returnValue(Promise.resolve('test-recaptcha-token'));

    TestBed.configureTestingModule({
    imports: [ReactiveFormsModule, ContactUsComponent],
    providers: [...standaloneComponentTestProviders, 
        { provide: ContactService, useValue: contactService },
        { provide: RecaptchaService, useValue: recaptchaService },
        { provide: ToastrService, useValue: toastr },
        { provide: AuthServiceService, useValue: authService }
    ]
}).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ContactUsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should create a form with 4 controls', () => {
    expect(component.contactForm.contains('name')).toBeTrue();
    expect(component.contactForm.contains('email')).toBeTrue();
    expect(component.contactForm.contains('phone')).toBeTrue();
    expect(component.contactForm.contains('message')).toBeTrue();
  });

  it('should be invalid when required fields are empty', () => {
    expect(component.contactForm.valid).toBeFalse();
  });

  it('should be valid when all required fields are populated', () => {
    component.contactForm.setValue({
      name: 'Jane Doe',
      email: 'jane@example.com',
      phone: '5551234567',
      message: 'Hello there'
    });

    expect(component.contactForm.valid).toBeTrue();
  });

  it('should reject messages longer than 2000 characters', () => {
    component.contactForm.setValue({
      name: 'Jane Doe',
      email: 'jane@example.com',
      phone: '',
      message: 'a'.repeat(2001)
    });

    expect(component.contactForm.valid).toBeFalse();
    expect(component.message?.hasError('maxlength')).toBeTrue();
  });

  it('should call ContactService.submitInquiry with form values and reCAPTCHA token on submit', () => {
    contactService.submitInquiry.and.returnValue(of({}));

    component.contactForm.setValue({
      name: 'Jane Doe',
      email: 'jane@example.com',
      phone: '5551234567',
      message: 'Need help'
    });

    component.onSubmit();

    expect(recaptchaService.execute).toHaveBeenCalledWith('contact_submit');
    expect(authService.ensureCsrfToken).toHaveBeenCalled();
    expect(contactService.submitInquiry).toHaveBeenCalledWith({
      name: 'Jane Doe',
      email: 'jane@example.com',
      phone: '5551234567',
      message: 'Need help',
      recaptchaToken: 'test-recaptcha-token'
    });
  });

  it('should show success toast and reset form after successful submission', () => {
    contactService.submitInquiry.and.returnValue(of({}));

    component.contactForm.setValue({
      name: 'Jane Doe',
      email: 'jane@example.com',
      phone: '',
      message: 'Need help'
    });

    component.onSubmit();

    expect(toastr.success).toHaveBeenCalledWith('Your inquiry has been submitted successfully.');
    expect(component.contactForm.value).toEqual({
      name: null,
      email: null,
      phone: null,
      message: null
    });
    expect(component.isSubmitting).toBeFalse();
  });

  it('should show error toast and preserve form values on submission failure', () => {
    contactService.submitInquiry.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 500 }))
    );

    const formValues = {
      name: 'Jane Doe',
      email: 'jane@example.com',
      phone: '5551234567',
      message: 'Need help'
    };
    component.contactForm.setValue(formValues);

    component.onSubmit();

    expect(toastr.error).toHaveBeenCalledWith(
      'Unable to submit your inquiry. Please check your details and try again.'
    );
    expect(component.contactForm.getRawValue()).toEqual(formValues);
    expect(component.isSubmitting).toBeFalse();
  });

  it('should show rate limit toast on 429 response', () => {
    contactService.submitInquiry.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 429 }))
    );

    component.contactForm.setValue({
      name: 'Jane Doe',
      email: 'jane@example.com',
      phone: '',
      message: 'Need help'
    });

    component.onSubmit();

    expect(toastr.error).toHaveBeenCalledWith('Too many requests. Please try again in a moment.');
  });

  it('should disable submit button when form is invalid', () => {
    fixture.detectChanges();
    const submitButton: HTMLButtonElement = fixture.nativeElement.querySelector('button[type="submit"]');
    expect(submitButton.disabled).toBeTrue();
  });

  it('should disable submit button while submission is in progress', () => {
    contactService.submitInquiry.and.returnValue(of({}));

    component.contactForm.setValue({
      name: 'Jane Doe',
      email: 'jane@example.com',
      phone: '',
      message: 'Need help'
    });

    component.isSubmitting = true;
    fixture.detectChanges();

    const submitButton: HTMLButtonElement = fixture.nativeElement.querySelector('button[type="submit"]');
    expect(submitButton.disabled).toBeTrue();
  });
});

import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Subject, Subscription, switchMap, takeUntil } from 'rxjs';
import { from } from 'rxjs';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { ContactInquiryDTO, ContactService } from '../services/contact.service';
import { RecaptchaService } from '../services/recaptcha.service';
import { RouterLink } from '@angular/router';
import { NgClass } from '@angular/common';

@Component({
    selector: 'app-contact-us',
    templateUrl: './contact-us.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./contact-us.component.css'],
    imports: [RouterLink, ReactiveFormsModule, NgClass]
})
export class ContactUsComponent implements OnInit, OnDestroy {
  contactForm!: FormGroup;
  isSubmitting = false;

  private readonly destroy$ = new Subject<void>();
  private submitSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private contactService: ContactService,
    private recaptchaService: RecaptchaService,
    private toastr: ToastrService,
    private authService: AuthServiceService
  ) {}

  ngOnInit(): void {
    this.contactForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: [''],
      message: ['', [Validators.required, Validators.maxLength(2000)]]
    });

    this.authService.ensureCsrfToken().pipe(takeUntil(this.destroy$)).subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.submitSubscription?.unsubscribe();
  }

  get name() {
    return this.contactForm.get('name');
  }

  get email() {
    return this.contactForm.get('email');
  }

  get phone() {
    return this.contactForm.get('phone');
  }

  get message() {
    return this.contactForm.get('message');
  }

  onSubmit(): void {
    if (this.contactForm.invalid || this.isSubmitting) {
      this.contactForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValues = this.contactForm.getRawValue();

    this.submitSubscription = this.authService.ensureCsrfToken().pipe(
      switchMap(() => from(this.recaptchaService.execute('contact_submit'))),
      switchMap((recaptchaToken) => {
        const inquiry: ContactInquiryDTO = {
          ...formValues,
          recaptchaToken
        };
        return this.contactService.submitInquiry(inquiry);
      })
    ).subscribe({
      next: () => {
        this.toastr.success('Your inquiry has been submitted successfully.');
        this.contactForm.reset();
        this.isSubmitting = false;
      },
      error: (error: HttpErrorResponse) => {
        this.handleSubmitError(error);
        this.isSubmitting = false;
      }
    });
  }

  private handleSubmitError(error: HttpErrorResponse): void {
    if (error.status === 429) {
      this.toastr.error('Too many requests. Please try again in a moment.');
      return;
    }

    this.toastr.error('Unable to submit your inquiry. Please check your details and try again.');
  }
}

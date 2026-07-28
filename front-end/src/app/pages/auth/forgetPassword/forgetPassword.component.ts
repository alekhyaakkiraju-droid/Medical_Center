import { Component, HostListener, OnDestroy, OnInit, ChangeDetectionStrategy, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { A11yModule } from '@angular/cdk/a11y';
import { ForgotServiceService } from '../auth-services/forgot-service.service';
import { ModelService } from '../auth-services/model.service';
import { ToastrService } from 'ngx-toastr';
import { NgClass } from '@angular/common';
import { ModalFocusService } from '../../../shared/services/modal-focus.service';

@Component({
    selector: 'app-forgetPassword',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './forgetPassword.component.html',
    imports: [ReactiveFormsModule, NgClass, A11yModule]
})
export class ForgetPasswordComponent implements OnInit, OnDestroy {

  isDialogOpen = false;
  isDialogMounted = false;
  forgetForm!: FormGroup;
  private modalSubscription!: Subscription;
  private readonly modalFocus = inject(ModalFocusService);

  constructor(private forgetpasswordService: ForgotServiceService,
              private modalService: ModelService,
              private toaster: ToastrService, private fb: FormBuilder) {
    this.forgetForm = this.fb.group({ emailForgot: ['', [Validators.required, Validators.email]] });
  }

  ngOnInit() {
    this.modalSubscription = this.modalService.dialogState$.subscribe((state) => {
      if (state && !this.isDialogOpen) {
        this.modalFocus.open();
      } else if (!state && this.isDialogOpen) {
        this.modalFocus.close();
      }
      this.isDialogOpen = state;
      this.isDialogMounted = state;
    });
  }
  ngOnDestroy(): void {
    this.modalSubscription?.unsubscribe();
    this.modalFocus.close();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isDialogOpen) {
      this.closeDialog();
    }
  }

  openDialog(): void {
    this.isDialogOpen = true;
    setTimeout(() => {
      this.isDialogMounted = true;
    }, 10);
  }

  closeDialog(): void {
    this.modalService.closeDialog();
  }

  get Forgotemail() {
    return this.forgetForm.get('emailForgot');
  }

  onForgotSubmit() {
    const emailForgetVal = this.forgetForm.value.emailForgot;
    this.forgetpasswordService.forgetPassword(emailForgetVal).subscribe({
      next: (res) => {
        this.toaster.success(`Success: ${res.message}`);
      },
      error: (err) => this.toaster.error(`Error: ${err.message}`)
    });
  }


}

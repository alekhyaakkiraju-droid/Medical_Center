import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ForgotServiceService } from '../auth-services/forgot-service.service';
import { ModelService } from '../auth-services/model.service';
import { ToastrService } from 'ngx-toastr';
import { NgClass } from '@angular/common';

@Component({
    selector: 'app-forgetPassword',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './forgetPassword.component.html',
    imports: [ReactiveFormsModule, NgClass]
})
export class ForgetPasswordComponent implements OnInit, OnDestroy {

  isDialogOpen = false;
  isDialogMounted = false;
  forgetForm!: FormGroup;
  private modalSubscription!: Subscription;

  constructor(private forgetpasswordService: ForgotServiceService,
              private modalService: ModelService,
              private toaster: ToastrService, private fb: FormBuilder) {
    this.forgetForm = this.fb.group({ emailForgot: ['', [Validators.required, Validators.email]] });
  }

  ngOnInit() {
    this.modalSubscription = this.modalService.dialogState$.subscribe((state) => {
      this.isDialogOpen = state;
      this.isDialogMounted = state;
    });
  }
  ngOnDestroy(): void {
    if (this.modalSubscription) {
      this.modalSubscription.unsubscribe();
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
    console.log("emailForgot", emailForgetVal);
    this.forgetpasswordService.forgetPassword(emailForgetVal).subscribe({
      next: (res) => {
        this.toaster.success(`Success: ${res.message}`);
      },
      error: (err) => this.toaster.error(`Error: ${err.message}`)
    });
  }


}

import { Component, HostListener, OnInit, ChangeDetectionStrategy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { A11yModule } from '@angular/cdk/a11y';
import { SnakebarService } from '../../../shared/service/SnakebarService.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { Router } from '@angular/router';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { ToastrService } from 'ngx-toastr';
import { ChangePasswordService } from '../services/change-password.service';
import { ProfileService } from '../services/Profile.service';
import { Profile, ProfileDetails } from '../../models';
import { NgClass } from '@angular/common';
import { ModalFocusService } from '../../../shared/services/modal-focus.service';

@Component({
    selector: 'app-user-profile',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './user-profile.component.html',
    imports: [ReactiveFormsModule, NgClass, A11yModule]
})
export class UserProfileComponent implements OnInit {
  private readonly modalFocus = inject(ModalFocusService);

  constructor(private fb: FormBuilder,
    private profileService: ProfileService,
    private reload: ReloadService,
    private router: Router,
    private toastr : ToastrService,
    private changePasswordService :ChangePasswordService,
    ) { }

  ngOnInit() {
    //this.cartSubscription =
    this.initForm();
    this.getProfileDetails();
   // this.onSubmit();
  }

ngAfterViewInit(): void {
this.reload.initializeLoader();
}
  isDialogOpen = false;
  isDialogMounted = false;
  openDialog(): void {
    this.modalFocus.open();
    this.isDialogOpen = true;
    setTimeout(() => {
      this.isDialogMounted = true;
    }, 10);
  }
  closeDialog(): void {
    this.isDialogMounted = false;
    setTimeout(() => {
      this.isDialogOpen = false;
      this.modalFocus.close();
    }, 150);
  }
  confirm(): void {
    this.closeDialog();
  }


  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  successMessage: string = '';
  errorMessage: string = '';


  get userName() {
    return this.profileForm.get('userName');
  }

  get email() {
    return this.profileForm.get('email');
  }

  get address() {
    return this.profileForm.get('address');
  }

  get coverImgUrl() {
    return this.profileForm.get('coverImgUrl');
  }
  get personalImgUrl() {
    return this.profileForm.get('personalImgUrl');
  }
  get phoneNumber() {
    return this.profileForm.get('phoneNumber');
  }

  get currentPassword() {
    return this.profileForm.get('userName');
  }

  get newPassword() {
    return this.profileForm.get('email');
  }

  get confirmPassword() {
    return this.profileForm.get('address');
  }


  profileData: ProfileDetails = {
    email: '',
    userName: '',
    phoneNumber: '',
    address:'',
    coverImgUrl: '',
    personalImgUrl: ''
  }

  initForm(): void {
    this.profileForm = this.fb.group({
      userName: [''],
      email: ['', [Validators.required, Validators.email]],
      address: [''],
      phoneNumber: [''],
      personalImgUrl: [''],
      coverImgUrl: ['']
    });


    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', Validators.required],
      confirmPassword: ['', Validators.required],
    }, { validators: this.passwordMatchValidator });
    
  }


  getProfileDetails(): void {
    this.profileService.getProfileDetails2().subscribe({
      next: (profile) => {
        this.profileData = profile;
       this.profileForm.patchValue(profile);  // Populate form with fetched data
      },
      error: (error) => {
      }
    });
  }

  onSubmit(): void {
    const profileInfo: Profile = this.profileForm.value;
    this.profileService.updateProfileDetails(profileInfo).subscribe({
      next: (response) => {
        this.toastr.success('Profile updated successfully.');

      },
      error: (error) => {
        this.toastr.error('Error updating profile.');

      }
    });
  }



  passwordMatchValidator(form: FormGroup) {
    return form.get('newPassword')?.value === form.get('confirmPassword')?.value ? null : { passwordMismatch: true };          
  }
  
  onChangePassword() {
    if (this.passwordForm.valid) {
      const model = {
        currentPassword: this.passwordForm.value.currentPassword,
        newPassword: this.passwordForm.value.newPassword,
      };
      this.changePasswordService.changePassword(model).subscribe({
        next: (response) => {
          this.successMessage = response;
          this.errorMessage = '';
          this.passwordForm.reset();
          this.toastr.success('Password updated successfully');

        },
        error: (error) => {
          this.errorMessage = error.error?.description || 'An unexpected error occurred.';
          this.successMessage = '';
          this.toastr.error(`Error updating password: ${this.errorMessage}`);

        },
      });
    }
  }
  isDialogOpen2 = false;
  isDialogMounted2 = false;
  openDialog2(): void {
    this.modalFocus.open();
    this.isDialogOpen2 = true;
    setTimeout(() => {
      this.isDialogMounted2 = true;
    }, 10);
  }
  closeDialog2(): void {
    this.isDialogMounted2 = false;
    setTimeout(() => {
      this.isDialogOpen2 = false;
      this.modalFocus.close();
    }, 150);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isDialogOpen2) {
      this.closeDialog2();
    } else if (this.isDialogOpen) {
      this.closeDialog();
    }
  }
}

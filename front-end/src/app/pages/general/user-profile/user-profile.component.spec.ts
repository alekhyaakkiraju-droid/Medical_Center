import { waitForAsync, ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { UserProfileComponent } from './user-profile.component';
import { ProfileService } from '../services/Profile.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ChangePasswordService } from '../services/change-password.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';
import { mockProfileDetails } from '../../../../testing/mock-data';

describe('UserProfileComponent', () => {
  let component: UserProfileComponent;
  let fixture: ComponentFixture<UserProfileComponent>;
  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [UserProfileComponent],
      providers: [
        ...standaloneComponentTestProviders,
        { provide: ProfileService, useValue: { getProfileDetails2: () => of(mockProfileDetails()), updateProfileDetails: () => of(mockProfileDetails()) } },
        { provide: ReloadService, useValue: { initializeLoader: () => undefined } },
        { provide: Router, useValue: { navigate: () => Promise.resolve(true) } },
        { provide: ToastrService, useValue: { success: () => undefined, error: () => undefined } },
        { provide: ChangePasswordService, useValue: { changePassword: () => of('ok') } },
      ],
    }).compileComponents();
  }));
  beforeEach(() => { fixture = TestBed.createComponent(UserProfileComponent); component = fixture.componentInstance; fixture.detectChanges(); });
  it('should create', () => { expect(component).toBeTruthy(); });
  it('displays fetched profile data on the form', () => { expect(component.profileData.email).toBe('user@example.com'); expect(component.profileForm.get('email')?.value).toBe('user@example.com'); });
  it('keeps username synchronized with profile details', () => { expect(component.profileForm.get('userName')?.value).toBe('Test User'); });
});

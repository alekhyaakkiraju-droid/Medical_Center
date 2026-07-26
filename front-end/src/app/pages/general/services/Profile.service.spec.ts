import { TestBed, inject } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { environment } from '../../../../environments/environment';
import { ProfileService } from './Profile.service';
import { HandleErrorsService } from '../../../shared/service/handle-errors.service';

describe('ProfileService', () => {
  let httpMock: HttpTestingController;
  beforeEach(() => { TestBed.configureTestingModule({ providers: [ProfileService, HandleErrorsService,         {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({
              headers: { 'Content-Type': 'application/json' },
              withCredentials: true
            })
          }
        }, provideHttpClient(), provideHttpClientTesting()] }); httpMock = TestBed.inject(HttpTestingController); });
  afterEach(() => httpMock.verify());
  it('getProfileDetails2 returns typed user details', inject([ProfileService], (service: ProfileService) => {
    service.getProfileDetails2().subscribe((result) => { expect(result.email).toBe('user@example.com'); });
    httpMock.expectOne(`${environment.api}/Account/user-details`).flush({ email: 'user@example.com', userName: 'user' });
  }));
  it('updateProfileDetails returns text response', inject([ProfileService], (service: ProfileService) => {
    service.updateProfileDetails({ userName: 'user', email: 'user@example.com' }).subscribe((result) => { expect(result).toBe('Profile updated'); });
    httpMock.expectOne(`${environment.api}/Account/update-profile`).flush('Profile updated');
  }));
});

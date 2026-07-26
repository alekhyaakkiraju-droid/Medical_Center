import { TestBed, inject } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { environment } from '../../../environments/environment';
import { TotalEarningsService } from './total-earnings.service';

describe('TotalEarningsService', () => {
  let httpMock: HttpTestingController;
  beforeEach(() => { TestBed.configureTestingModule({ providers: [TotalEarningsService,         {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({
              headers: { 'Content-Type': 'application/json' },
              withCredentials: true
            })
          }
        }, provideHttpClient(), provideHttpClientTesting()] }); httpMock = TestBed.inject(HttpTestingController); });
  afterEach(() => httpMock.verify());
  it('getTotalEarnings returns typed response', inject([TotalEarningsService], (service: TotalEarningsService) => {
    service.getTotalEarnings().subscribe((result) => { expect(result.totalEarnings).toBe(12500); });
    httpMock.expectOne(`${environment.api}/Appointments/total-earnings`).flush({ totalEarnings: 12500 });
  }));
});

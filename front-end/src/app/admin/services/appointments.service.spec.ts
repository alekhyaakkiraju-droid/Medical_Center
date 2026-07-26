import { TestBed, inject } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { environment } from '../../../environments/environment';
import { AppointmentsService } from './appointments.service';

describe('AppointmentsService', () => {
  let httpMock: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [AppointmentsService,         {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({
              headers: { 'Content-Type': 'application/json' },
              withCredentials: true
            })
          }
        }, provideHttpClient(), provideHttpClientTesting()] });
    httpMock = TestBed.inject(HttpTestingController);
  });
  afterEach(() => httpMock.verify());
  it('getAppointments returns a typed paged result', inject([AppointmentsService], (service: AppointmentsService) => {
    service.getAppointments().subscribe((result) => { expect(result.items?.[0]?.appointmentId).toBe(1); });
    httpMock.expectOne(`${environment.api}/Appointments`).flush({ items: [{ appointmentId: 1 }], totalCount: 1, pageCount: 1, currentPage: 1, pageSize: 20 });
  }));
});

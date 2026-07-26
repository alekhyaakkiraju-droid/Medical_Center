import { TestBed, inject } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { environment } from '../../../environments/environment';
import { PatientService } from './patient.service';

describe('PatientService', () => {
  let httpMock: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PatientService,         {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({
              headers: { 'Content-Type': 'application/json' },
              withCredentials: true
            })
          }
        }, provideHttpClient(), provideHttpClientTesting()],
    });
    httpMock = TestBed.inject(HttpTestingController);
  });
  afterEach(() => httpMock.verify());
  it('getAllPatient returns a typed paged result', inject([PatientService], (service: PatientService) => {
    service.getAllPatient().subscribe((result) => {
      expect(result.items?.[0]?.patientId).toBe('p1');
      expect(result.totalCount).toBe(1);
    });
    httpMock.expectOne(`${environment.api}/Patients`).flush({ items: [{ patientId: 'p1' }], totalCount: 1, pageCount: 1, currentPage: 1, pageSize: 20 });
  }));
});

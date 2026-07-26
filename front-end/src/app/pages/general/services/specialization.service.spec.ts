import { TestBed, inject } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { environment } from '../../../../environments/environment';
import { SpecializationService } from './specialization.service';

describe('SpecializationService', () => {
  let httpMock: HttpTestingController;
  beforeEach(() => { TestBed.configureTestingModule({ providers: [SpecializationService,         {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({
              headers: { 'Content-Type': 'application/json' },
              withCredentials: true
            })
          }
        }, provideHttpClient(), provideHttpClientTesting()] }); httpMock = TestBed.inject(HttpTestingController); });
  afterEach(() => httpMock.verify());
  it('getSpecializations returns typed paged result', inject([SpecializationService], (service: SpecializationService) => {
    service.getSpecializations().subscribe((result) => { expect(result.items?.[0]?.specializationName).toBe('Cardiology'); });
    httpMock.expectOne((request) => request.url.startsWith(`${environment.api}/Specializations`)).flush({ items: [{ id: 1, specializationName: 'Cardiology' }], totalCount: 1, pageCount: 1, currentPage: 1, pageSize: 100 });
  }));
});

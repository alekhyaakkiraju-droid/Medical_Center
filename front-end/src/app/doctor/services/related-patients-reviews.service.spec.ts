import { TestBed, inject } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { environment } from '../../../environments/environment';
import { RelatedPatientsReviewsService } from './related-patients-reviews.service';

describe('RelatedPatientsReviewsService', () => {
  let httpMock: HttpTestingController;
  beforeEach(() => { TestBed.configureTestingModule({ providers: [RelatedPatientsReviewsService,         {
          provide: AuthServiceService,
          useValue: {
            getHttpOptions: () => ({
              headers: { 'Content-Type': 'application/json' },
              withCredentials: true
            })
          }
        }, provideHttpClient(), provideHttpClientTesting()] }); httpMock = TestBed.inject(HttpTestingController); });
  afterEach(() => httpMock.verify());
  it('getPatientsReview returns typed paged result', inject([RelatedPatientsReviewsService], (service: RelatedPatientsReviewsService) => {
    service.getPatientsReview('doc-1').subscribe((result) => { expect(result.items?.[0]?.review).toBe('Great doctor'); });
    httpMock.expectOne(`${environment.api}/Doctors/doc-1/reviews`).flush({ items: [{ id: 1, review: 'Great doctor' }], totalCount: 1, pageCount: 1, currentPage: 1, pageSize: 20 });
  }));
});

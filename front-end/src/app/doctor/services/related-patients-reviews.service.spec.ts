/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { RelatedPatientsReviewsService } from './related-patients-reviews.service';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';

describe('Service: PatientsReviews', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, RelatedPatientsReviewsService]
    });
  });

  it('should ...', inject([RelatedPatientsReviewsService], (service: RelatedPatientsReviewsService) => {
    expect(service).toBeTruthy();
  }));
});

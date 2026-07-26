/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { SpecializationService } from './specialization.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('Service: Specialization', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, SpecializationService]
    });
  });

  it('should ...', inject([SpecializationService], (service: SpecializationService) => {
    expect(service).toBeTruthy();
  }));
});

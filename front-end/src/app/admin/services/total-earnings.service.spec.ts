/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { TotalEarningsService } from './total-earnings.service';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';

describe('Service: TotalEarnings', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, TotalEarningsService]
    });
  });

  it('should ...', inject([TotalEarningsService], (service: TotalEarningsService) => {
    expect(service).toBeTruthy();
  }));
});

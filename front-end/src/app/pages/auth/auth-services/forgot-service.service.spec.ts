/* tslint:disable:no-unused-variable */

import { TestBed,  inject } from '@angular/core/testing';
import { ForgotServiceService } from './forgot-service.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('Service: ForgotService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, ForgotServiceService]
    });
  });

  it('should ...', inject([ForgotServiceService], (service: ForgotServiceService) => {
    expect(service).toBeTruthy();
  }));
});

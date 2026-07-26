/* tslint:disable:no-unused-variable */

import { TestBed,  inject } from '@angular/core/testing';
import { HandleErrorsService } from './handle-errors.service';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';

describe('Service: HandleErrors', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, HandleErrorsService]
    });
  });

  it('should ...', inject([HandleErrorsService], (service: HandleErrorsService) => {
    expect(service).toBeTruthy();
  }));
});

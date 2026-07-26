/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { ModelService } from './model.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('Service: Model', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, ModelService]
    });
  });

  it('should ...', inject([ModelService], (service: ModelService) => {
    expect(service).toBeTruthy();
  }));
});

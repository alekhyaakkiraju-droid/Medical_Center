/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { FlowbiteService } from './Flowbite.service';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';

describe('Service: Flowbite', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, FlowbiteService]
    });
  });

  it('should ...', inject([FlowbiteService], (service: FlowbiteService) => {
    expect(service).toBeTruthy();
  }));
});

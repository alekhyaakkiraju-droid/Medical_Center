/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { DoctorService } from './doctor.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('Service: Doctor', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, DoctorService]
    });
  });

  it('should ...', inject([DoctorService], (service: DoctorService) => {
    expect(service).toBeTruthy();
  }));
});

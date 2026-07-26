/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { AppointmentsService } from './appointments.service';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';

describe('Service: Appointments', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, AppointmentsService]
    });
  });

  it('should ...', inject([AppointmentsService], (service: AppointmentsService) => {
    expect(service).toBeTruthy();
  }));
});

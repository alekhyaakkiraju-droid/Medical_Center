/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { DoctorAppointmentsService } from './doctor-appointments.service';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';

describe('Service: DoctorAppointments', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, DoctorAppointmentsService]
    });
  });

  it('should ...', inject([DoctorAppointmentsService], (service: DoctorAppointmentsService) => {
    expect(service).toBeTruthy();
  }));
});

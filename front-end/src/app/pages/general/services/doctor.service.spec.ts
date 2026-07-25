/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { DoctorService } from './doctor.service';

describe('Service: Doctor', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DoctorService]
    });
  });

  it('should ...', inject([DoctorService], (service: DoctorService) => {
    expect(service).toBeTruthy();
  }));
});

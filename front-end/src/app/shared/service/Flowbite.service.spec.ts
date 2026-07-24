/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { FlowbiteService } from './Flowbite.service';

describe('Service: Flowbite', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [FlowbiteService]
    });
  });

  it('should ...', inject([FlowbiteService], (service: FlowbiteService) => {
    expect(service).toBeTruthy();
  }));
});

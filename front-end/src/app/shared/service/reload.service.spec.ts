/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { ReloadService } from './reload.service';

describe('Service: Reload', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ReloadService]
    });
  });

  it('should ...', inject([ReloadService], (service: ReloadService) => {
    expect(service).toBeTruthy();
  }));
});

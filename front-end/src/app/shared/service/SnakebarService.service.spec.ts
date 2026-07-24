/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { SnakebarService } from './SnakebarService.service';

describe('Service: SnakebarService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SnakebarService]
    });
  });

  it('should ...', inject([SnakebarService], (service: SnakebarService) => {
    expect(service).toBeTruthy();
  }));
});

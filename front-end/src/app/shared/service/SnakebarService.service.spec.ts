/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { SnakebarService } from './SnakebarService.service';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';

describe('Service: SnakebarService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, SnakebarService]
    });
  });

  it('should ...', inject([SnakebarService], (service: SnakebarService) => {
    expect(service).toBeTruthy();
  }));
});

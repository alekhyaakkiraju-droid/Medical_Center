/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { ChangePasswordService } from './change-password.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('Service: ChangePassword', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, ChangePasswordService]
    });
  });

  it('should ...', inject([ChangePasswordService], (service: ChangePasswordService) => {
    expect(service).toBeTruthy();
  }));
});

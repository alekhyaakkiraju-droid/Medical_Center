/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { ResetPasswordService } from './resetPassword.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('Service: ResetPassword', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, ResetPasswordService]
    });
  });

  it('should ...', inject([ResetPasswordService], (service: ResetPasswordService) => {
    expect(service).toBeTruthy();
  }));
});

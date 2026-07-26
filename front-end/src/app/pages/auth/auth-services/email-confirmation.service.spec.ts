/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { EmailConfirmationService } from './email-confirmation.service';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('Service: EmailConfirmation', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, EmailConfirmationService]
    });
  });

  it('should ...', inject([EmailConfirmationService], (service: EmailConfirmationService) => {
    expect(service).toBeTruthy();
  }));
});

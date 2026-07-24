/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { EmailConfirmationService } from './email-confirmation.service';

describe('Service: EmailConfirmation', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [EmailConfirmationService]
    });
  });

  it('should ...', inject([EmailConfirmationService], (service: EmailConfirmationService) => {
    expect(service).toBeTruthy();
  }));
});

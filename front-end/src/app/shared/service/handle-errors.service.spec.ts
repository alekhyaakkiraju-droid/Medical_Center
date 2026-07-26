import { TestBed, inject } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { HandleErrorsService } from './handle-errors.service';

describe('HandleErrorsService', () => {
  beforeEach(() => { TestBed.configureTestingModule({ providers: [HandleErrorsService] }); });
  it('handleError accepts HttpErrorResponse', inject([HandleErrorsService], (service: HandleErrorsService) => {
    const error = new HttpErrorResponse({ status: 500, statusText: 'Server Error' });
    service.handleError(error).subscribe({ next: () => fail('expected error'), error: (err: Error) => { expect(err.message).toContain('Something went wrong'); } });
  }));
});

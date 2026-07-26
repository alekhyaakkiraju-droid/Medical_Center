/* tslint:disable:no-unused-variable */
import { waitForAsync, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { PatientReviewsComponent } from './patient-reviews.component';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('PatientReviewsComponent', () => {
  let component: PatientReviewsComponent;
  let fixture: ComponentFixture<PatientReviewsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
    imports: [PatientReviewsComponent],
    providers: standaloneComponentTestProviders,
})
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PatientReviewsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

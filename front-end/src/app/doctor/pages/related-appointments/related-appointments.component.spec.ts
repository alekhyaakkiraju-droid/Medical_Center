/* tslint:disable:no-unused-variable */
import { waitForAsync, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { RelatedAppointmentsComponent } from './related-appointments.component';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('RelatedAppointmentsComponent', () => {
  let component: RelatedAppointmentsComponent;
  let fixture: ComponentFixture<RelatedAppointmentsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
    imports: [RelatedAppointmentsComponent],
    providers: standaloneComponentTestProviders,
})
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RelatedAppointmentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

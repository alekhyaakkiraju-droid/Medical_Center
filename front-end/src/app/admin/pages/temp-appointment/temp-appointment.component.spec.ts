/* tslint:disable:no-unused-variable */
import { waitForAsync, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { TempAppointmentComponent } from './temp-appointment.component';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('TempAppointmentComponent', () => {
  let component: TempAppointmentComponent;
  let fixture: ComponentFixture<TempAppointmentComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
    imports: [TempAppointmentComponent],
    providers: standaloneComponentTestProviders,
})
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TempAppointmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

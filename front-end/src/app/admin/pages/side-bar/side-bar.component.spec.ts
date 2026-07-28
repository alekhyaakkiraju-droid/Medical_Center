/* tslint:disable:no-unused-variable */
import { waitForAsync, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { SideBarComponent } from './side-bar.component';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';
import { AssetUrlPipe } from '../../../shared/asset-url.pipe';

describe('SideBarComponent', () => {
  let component: SideBarComponent;
  let fixture: ComponentFixture<SideBarComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
    imports: [SideBarComponent],
    providers: standaloneComponentTestProviders,
})
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SideBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('includes AssetUrlPipe for admin logo binding', () => {
    expect(new AssetUrlPipe()).toBeTruthy();
    const logo = fixture.debugElement.query(By.css('img'));
    expect(logo).toBeTruthy();
    expect(logo.nativeElement.getAttribute('src')).toContain('loggggo-3.png');
  });
});

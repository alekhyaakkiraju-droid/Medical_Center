/* tslint:disable:no-unused-variable */
import { waitForAsync, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { HeaderComponent } from './header.component';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';

describe('HeaderComponent', () => {
  let component: HeaderComponent;
  let fixture: ComponentFixture<HeaderComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
    imports: [HeaderComponent],
    providers: standaloneComponentTestProviders,
})
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('includes CareShift text logo in header', () => {
    const logo = fixture.nativeElement.querySelector('.careshift-logo');
    expect(logo).toBeTruthy();
    expect(logo.textContent).toContain('CareShift');
  });

  it('should label ribbon social links for screen reader users', () => {
    const socialAnchors = fixture.nativeElement.querySelectorAll('.ribbon-links__social');
    expect(socialAnchors.length).toBeGreaterThan(0);
    socialAnchors.forEach((anchor: HTMLAnchorElement) => {
      expect(anchor.getAttribute('aria-label')).toBeTruthy();
    });
  });

  it('should expose keyboard-focusable logout control when logged in', () => {
    component.isLoggedIn = true;
    fixture.detectChanges();
    const logoutButton = fixture.nativeElement.querySelector('.logout-trigger');
    expect(logoutButton?.tagName.toLowerCase()).toBe('button');
    expect(logoutButton?.getAttribute('aria-label')).toBe('Log out');
  });
});

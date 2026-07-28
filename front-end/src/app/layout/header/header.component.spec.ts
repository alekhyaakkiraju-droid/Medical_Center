/* tslint:disable:no-unused-variable */
import { waitForAsync, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { HeaderComponent } from './header.component';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';
import { AssetUrlPipe } from '../../shared/asset-url.pipe';

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

  it('includes AssetUrlPipe in standalone imports', () => {
    expect(HeaderComponent).toBeDefined();
    expect(new AssetUrlPipe()).toBeTruthy();
    const logo = fixture.debugElement.query(By.css('.logo img'));
    expect(logo).toBeTruthy();
    expect(logo.nativeElement.getAttribute('src')).toContain('loggggo-3.png');
  });

  it('should label social links for keyboard and screen reader users', () => {
    const socialAnchors = fixture.nativeElement.querySelectorAll('.social-links a');
    expect(socialAnchors.length).toBeGreaterThan(0);
    socialAnchors.forEach((anchor: HTMLAnchorElement) => {
      expect(anchor.getAttribute('aria-label')).toMatch(/Visit our .+ page/);
      expect(anchor.tabIndex).toBeGreaterThanOrEqual(0);
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

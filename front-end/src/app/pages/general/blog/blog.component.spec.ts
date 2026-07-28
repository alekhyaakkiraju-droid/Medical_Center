/* tslint:disable:no-unused-variable */
import { waitForAsync, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { BlogComponent } from './blog.component';
import { standaloneComponentTestProviders } from '../../../testing/standalone-component-test-providers';

describe('BlogComponent', () => {
  let component: BlogComponent;
  let fixture: ComponentFixture<BlogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
    imports: [BlogComponent],
    providers: standaloneComponentTestProviders,
})
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BlogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should use keyboard-operable button pagination instead of hash links', () => {
    const paginationButtons = fixture.nativeElement.querySelectorAll('.styled-pagination button');
    expect(paginationButtons.length).toBeGreaterThan(0);
    paginationButtons.forEach((button: HTMLButtonElement) => {
      expect(button.type).toBe('button');
    });
    expect(fixture.nativeElement.querySelector('.styled-pagination a[href="#"]')).toBeNull();
  });

  it('should update current page when pagination is clicked', () => {
    const pageTwo = fixture.nativeElement.querySelector('.styled-pagination button[aria-label="Page 2"]');
    pageTwo?.click();
    fixture.detectChanges();
    expect(component.currentPage).toBe(2);
  });
});

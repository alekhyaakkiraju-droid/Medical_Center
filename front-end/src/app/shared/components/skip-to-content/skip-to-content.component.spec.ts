import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SkipToContentComponent } from './skip-to-content.component';

describe('SkipToContentComponent', () => {
  let fixture: ComponentFixture<SkipToContentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SkipToContentComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SkipToContentComponent);
    fixture.detectChanges();
  });

  it('should render skip link targeting main content', () => {
    const link = fixture.nativeElement.querySelector('a');
    expect(link?.getAttribute('href')).toBe('#main-content');
    expect(link?.textContent?.trim()).toBe('Skip to main content');
  });
});

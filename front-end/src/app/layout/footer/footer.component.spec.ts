import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { FooterComponent } from './footer.component';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';
import { AssetUrlPipe } from '../../shared/asset-url.pipe';

describe('FooterComponent', () => {
  let component: FooterComponent;
  let fixture: ComponentFixture<FooterComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [FooterComponent],
      providers: standaloneComponentTestProviders,
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FooterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('includes AssetUrlPipe for footer image bindings', () => {
    expect(new AssetUrlPipe()).toBeTruthy();
    const logo = fixture.debugElement.query(By.css('.about-widget img'));
    expect(logo).toBeTruthy();
    expect(logo.nativeElement.getAttribute('src')).toContain('loggggo-3.png');
  });
});

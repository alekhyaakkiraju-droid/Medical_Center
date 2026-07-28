import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SessionTimeoutWarningComponent } from './session-timeout-warning.component';

describe('SessionTimeoutWarningComponent', () => {
  let component: SessionTimeoutWarningComponent;
  let fixture: ComponentFixture<SessionTimeoutWarningComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SessionTimeoutWarningComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SessionTimeoutWarningComponent);
    component = fixture.componentInstance;
  });

  it('should not render modal when hidden', () => {
    component.isVisible = false;
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alertdialog"]')).toBeNull();
  });

  it('should render modal with ARIA attributes when visible', () => {
    component.isVisible = true;
    component.countdownSeconds = 45;
    fixture.detectChanges();

    const dialog = fixture.nativeElement.querySelector('[role="alertdialog"]');
    expect(dialog).toBeTruthy();
    expect(dialog.getAttribute('aria-modal')).toBe('true');
    expect(fixture.nativeElement.textContent).toContain('45');
  });

  it('should emit stayLoggedIn when button clicked', () => {
    const spy = jasmine.createSpy('stayLoggedIn');
    component.stayLoggedIn.subscribe(spy);

    component.isVisible = true;
    fixture.detectChanges();
    component.onStayLoggedIn();

    expect(spy).toHaveBeenCalled();
  });

  it('should emit logOut when button clicked', () => {
    const spy = jasmine.createSpy('logOut');
    component.logOut.subscribe(spy);

    component.onLogOut();

    expect(spy).toHaveBeenCalled();
  });
});

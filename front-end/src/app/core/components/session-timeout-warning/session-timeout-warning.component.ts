import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  ViewChild,
} from '@angular/core';

@Component({
  selector: 'app-session-timeout-warning',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './session-timeout-warning.component.html',
  imports: [],
})
export class SessionTimeoutWarningComponent implements OnChanges {
  @Input() isVisible = false;
  @Input() countdownSeconds: number | null = null;
  @Output() stayLoggedIn = new EventEmitter<void>();
  @Output() logOut = new EventEmitter<void>();

  @ViewChild('stayLoggedInButton') stayLoggedInButton?: ElementRef<HTMLButtonElement>;

  constructor() {
    afterNextRender(() => {
      if (this.isVisible) {
        this.focusStayLoggedInButton();
      }
    });
  }

  ngOnChanges(): void {
    if (this.isVisible) {
      queueMicrotask(() => this.focusStayLoggedInButton());
    }
  }

  onStayLoggedIn(): void {
    this.stayLoggedIn.emit();
  }

  onLogOut(): void {
    this.logOut.emit();
  }

  private focusStayLoggedInButton(): void {
    this.stayLoggedInButton?.nativeElement.focus();
  }
}

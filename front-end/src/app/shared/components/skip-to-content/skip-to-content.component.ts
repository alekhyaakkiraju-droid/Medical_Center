import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-skip-to-content',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<a class="visually-hidden" href="#main-content">Skip to main content</a>`,
})
export class SkipToContentComponent {}

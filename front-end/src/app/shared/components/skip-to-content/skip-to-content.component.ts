import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-skip-to-content',
  standalone: true,
  imports: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <a
      class="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-50 focus:bg-white focus:p-2 focus:text-lg focus:rounded focus:shadow-md"
      href="#main-content">
      Skip to main content
    </a>
  `,
})
export class SkipToContentComponent {}

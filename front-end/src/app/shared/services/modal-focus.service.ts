import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({ providedIn: 'root' })
export class ModalFocusService {
  private readonly platformId = inject(PLATFORM_ID);
  private triggerElement: HTMLElement | null = null;
  private isOpen = false;

  open(trigger?: HTMLElement | null): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.triggerElement = trigger ?? (document.activeElement as HTMLElement | null);
    document.getElementById('main-content')?.setAttribute('aria-hidden', 'true');
    this.isOpen = true;
  }

  close(): void {
    if (!isPlatformBrowser(this.platformId) || !this.isOpen) {
      return;
    }

    document.getElementById('main-content')?.removeAttribute('aria-hidden');
    this.triggerElement?.focus();
    this.triggerElement = null;
    this.isOpen = false;
  }
}

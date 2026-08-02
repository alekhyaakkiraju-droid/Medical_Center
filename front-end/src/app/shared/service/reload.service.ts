import { isPlatformBrowser } from '@angular/common';
import { Inject, Injectable, PLATFORM_ID } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ReloadService {

  constructor(@Inject(PLATFORM_ID) private readonly platformId: object) {}

  initializeLoader(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }
    const loader = document.querySelector('.loader');
    const preloader = document.getElementById('preloader');

    if (!loader && !preloader) {
      return;
    }

    if (loader) {
      loader.classList.remove('fade-in');
      loader.classList.add('fade-out');
      this.hidePreloader(preloader);
      return;
    }

    this.hidePreloader(preloader);
  }

  resetLoader(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }
    const loader = document.querySelector('.loader');
    const preloader = document.getElementById('preloader');

    if (!loader && !preloader) {
      return;
    }

    if (preloader) {
      preloader.style.display = 'block';
    }

    if (loader) {
      loader.classList.remove('fade-out');
      loader.classList.add('fade-in');
    }
  }

  private hidePreloader(preloader: HTMLElement | null): void {
    if (preloader) {
      preloader.style.display = 'none';
    }
  }
}

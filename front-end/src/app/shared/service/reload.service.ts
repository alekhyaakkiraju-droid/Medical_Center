import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ReloadService {

  initializeLoader(): void {
    const loader = document.querySelector('.loader');
    const preloader = document.getElementById('preloader');

    if (!loader && !preloader) {
      return;
    }

    if (loader) {
      loader.classList.remove('fade-in');
      setTimeout(() => {
        loader.classList.add('fade-out');
      }, 300);
      setTimeout(() => {
        this.hidePreloader(preloader);
      }, 600);
      return;
    }

    this.hidePreloader(preloader);
  }

  resetLoader(): void {
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

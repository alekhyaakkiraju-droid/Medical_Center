import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-image-lightbox',
  standalone: true,
  imports: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (imageSrc) {
      <div
        class="image-lightbox"
        role="dialog"
        aria-modal="true"
        [attr.aria-label]="title || 'Image preview'"
        (click)="close()"
        (keydown.escape)="close()"
        tabindex="-1"
      >
        <div class="image-lightbox__panel" (click)="$event.stopPropagation()">
          <button type="button" class="image-lightbox__close" (click)="close()" aria-label="Close preview">
            &times;
          </button>
          @if (title) {
            <p class="image-lightbox__title">{{ title }}</p>
          }
          <img [src]="imageSrc" [alt]="title || 'Expanded image'" />
        </div>
      </div>
    }
  `,
  styles: [`
    .image-lightbox {
      position: fixed;
      inset: 0;
      z-index: 100000;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 24px;
      background: rgba(0, 0, 0, 0.85);
    }

    .image-lightbox__panel {
      position: relative;
      max-width: min(960px, 100%);
      max-height: 90vh;
    }

    .image-lightbox__panel img {
      display: block;
      max-width: 100%;
      max-height: calc(90vh - 48px);
      width: auto;
      height: auto;
      margin: 0 auto;
      border-radius: 8px;
    }

    .image-lightbox__title {
      color: #fff;
      text-align: center;
      margin: 0 0 12px;
      font-size: 18px;
    }

    .image-lightbox__close {
      position: absolute;
      top: -40px;
      right: 0;
      border: none;
      background: transparent;
      color: #fff;
      font-size: 32px;
      line-height: 1;
      cursor: pointer;
    }
  `],
})
export class ImageLightboxComponent {
  imageSrc: string | null = null;
  title = '';

  open(imageSrc: string, title = ''): void {
    this.imageSrc = imageSrc;
    this.title = title;
  }

  close(): void {
    this.imageSrc = null;
    this.title = '';
  }
}

import { Component, OnInit, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CollectedShotsComponent } from '../collected-shots/collected-shots.component';
import { AssetUrlPipe } from '../../../shared/asset-url.pipe';
import { ImageLightboxComponent } from '../../../shared/image-lightbox/image-lightbox.component';
import { resolveAssetUrl } from '../../../shared/asset-url.util';

@Component({
    selector: 'app-gallery',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './gallery.component.html',
    imports: [RouterLink, CollectedShotsComponent, AssetUrlPipe, ImageLightboxComponent]
})
export class GalleryComponent implements OnInit {

  @ViewChild(ImageLightboxComponent) lightbox!: ImageLightboxComponent;

  constructor() { }

  ngOnInit() {
  }

  mediaGallery = [
    {
      thumbnail: 'images/gallery/video-thumb-01.jpg',
      fullImage: 'images/gallery/gallery-01.jpg',
      title: 'Medical Innovations Conference 2024',
    },
    {
      thumbnail: 'images/gallery/video-thumb-02.jpg',
      fullImage: 'images/gallery/gallery-02.jpg',
      title: 'Surgical Advancements Summit 2023',
    },
    {
      thumbnail: 'images/gallery/video-thumb-03.jpg',
      fullImage: 'images/gallery/gallery-03.jpg',
      title: 'Emergency Medicine Symposium 2022',
    },
    {
      thumbnail: 'images/gallery/video-thumb-04.jpg',
      fullImage: 'images/gallery/gallery-04.jpg',
      title: 'Healthcare Technology Forum 2021',
    },
  ];

  openMedia(item: (typeof this.mediaGallery)[number]): void {
    this.lightbox.open(resolveAssetUrl(item.fullImage), item.title);
  }
}

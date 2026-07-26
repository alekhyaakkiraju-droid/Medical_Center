import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CollectedShotsComponent } from '../collected-shots/collected-shots.component';
import { AssetUrlPipe } from '../../../shared/asset-url.pipe';

@Component({
    selector: 'app-gallery',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './gallery.component.html',
    imports: [RouterLink, CollectedShotsComponent, AssetUrlPipe]
})
export class GalleryComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }
  

  videoGallery = [
    {
      thumbnail: 'images/gallery/video-thumb-01.jpg',
      videoUrl: 'https://www.youtube.com/watch?v=h-h5Mhlt6O0',
      title: 'Medical Innovations Conference 2024',
    },
    {
      thumbnail: 'images/gallery/video-thumb-02.jpg',
      videoUrl: 'https://www.youtube.com/watch?v=h-h5Mhlt6O0',
      title: 'Surgical Advancements Summit 2023',
    },
    {
      thumbnail: 'images/gallery/video-thumb-03.jpg',
      videoUrl: 'https://www.youtube.com/watch?v=h-h5Mhlt6O0',
      title: 'Emergency Medicine Symposium 2022',
    },
    {
      thumbnail: 'images/gallery/video-thumb-04.jpg',
      videoUrl: 'https://www.youtube.com/watch?v=h-h5Mhlt6O0',
      title: 'Healthcare Technology Forum 2021',
    },
  ];

}

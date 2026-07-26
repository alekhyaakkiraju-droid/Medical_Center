import { Component, OnInit, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { AssetUrlPipe } from '../../../shared/asset-url.pipe';
import { ImageLightboxComponent } from '../../../shared/image-lightbox/image-lightbox.component';
import { resolveAssetUrl } from '../../../shared/asset-url.util';

@Component({
    selector: 'app-collected-shots',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './collected-shots.component.html',
    imports: [AssetUrlPipe, ImageLightboxComponent]
})
export class CollectedShotsComponent implements OnInit {

  @ViewChild(ImageLightboxComponent) lightbox!: ImageLightboxComponent;

  constructor() { }

  ngOnInit() {
  }

  galleryItems = [
    {
      image: 'images/gallery/gallery-01.jpg',
      title: 'Modern Operating Room',
      description: 'Equipped with state-of-the-art surgical tools to ensure precision and patient safety during operations.',
    },
    {
      image: 'images/gallery/gallery-02.jpg',
      title: 'Advanced Diagnostic Center',
      description: 'Our diagnostic center features MRI, CT scan, and X-ray facilities for accurate medical assessments.',
    },
    {
      image: 'images/gallery/gallery-03.jpg',
      title: 'Emergency Care Unit',
      description: '24/7 emergency response team with modern life-saving equipment to handle critical cases efficiently.',
    },
    {
      image: 'images/gallery/gallery-04.jpg',
      title: 'Pediatric Ward',
      description: 'A child-friendly environment with specialized pediatricians ensuring quality care for young patients.',
    },
    {
      image: 'images/gallery/gallery-05.jpg',
      title: 'Intensive Care Unit (ICU)',
      description: 'A high-tech ICU with advanced monitoring systems for critically ill patients needing constant care.',
    },
    {
      image: 'images/gallery/gallery-06.jpg',
      title: 'Pharmacy & Medication Center',
      description: 'Fully stocked with essential medicines and professional pharmacists to provide proper guidance.',
    },
  ];

  openImage(item: (typeof this.galleryItems)[number]): void {
    this.lightbox.open(resolveAssetUrl(item.image), item.title);
  }
}

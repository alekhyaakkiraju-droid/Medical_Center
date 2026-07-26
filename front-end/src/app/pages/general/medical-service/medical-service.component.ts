import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FaqComponent } from '../faq/faq.component';
import { AssetUrlPipe } from '../../../shared/asset-url.pipe';

@Component({
    selector: 'app-medical-service',
    templateUrl: './medical-service.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./medical-service.component.css'],
    imports: [RouterLink, FaqComponent, AssetUrlPipe]
})
export class MedicalServiceComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

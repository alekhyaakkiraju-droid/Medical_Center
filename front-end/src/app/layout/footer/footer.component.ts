import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AssetUrlPipe } from '../../shared/asset-url.pipe';

@Component({
    selector: 'app-footer',
    templateUrl: './footer.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./footer.component.css'],
    imports: [RouterLink, AssetUrlPipe]
})
export class FooterComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

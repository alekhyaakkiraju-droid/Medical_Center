import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CollectedShotsComponent } from '../collected-shots/collected-shots.component';

@Component({
    selector: 'app-about-us',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './about-us.component.html',
    imports: [RouterLink, CollectedShotsComponent]
})
export class AboutUsComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-footer',
    templateUrl: './footer.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./footer.component.css'],
    imports: [RouterLink]
})
export class FooterComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

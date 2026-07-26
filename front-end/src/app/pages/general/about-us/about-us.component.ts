import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-about-us',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './about-us.component.html'
})
export class AboutUsComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

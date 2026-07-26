import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-errorPage',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './errorPage.component.html'
})
export class ErrorPageComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

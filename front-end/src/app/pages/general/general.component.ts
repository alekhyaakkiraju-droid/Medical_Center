import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-general',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './general.component.html',
})
export class GeneralComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

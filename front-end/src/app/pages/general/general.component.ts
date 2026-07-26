import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-general',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './general.component.html',
    imports: []
})
export class GeneralComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

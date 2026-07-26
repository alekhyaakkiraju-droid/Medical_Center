import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-doctor',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './doctor.component.html',
    imports: []
})
export class DoctorComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

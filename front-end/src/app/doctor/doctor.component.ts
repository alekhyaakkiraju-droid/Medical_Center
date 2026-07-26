import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-doctor',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './doctor.component.html'
})
export class DoctorComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

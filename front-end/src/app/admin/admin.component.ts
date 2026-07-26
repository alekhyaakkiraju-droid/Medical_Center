import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-admin',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './admin.component.html'
})
export class AdminComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

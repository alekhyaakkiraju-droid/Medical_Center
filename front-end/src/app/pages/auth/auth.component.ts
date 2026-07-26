import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-auth',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './auth.component.html', 
})
export class AuthComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

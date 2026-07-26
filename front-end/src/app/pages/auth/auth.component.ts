import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-auth',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './auth.component.html',
    imports: []
})
export class AuthComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

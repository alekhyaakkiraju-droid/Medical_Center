import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-medical-service',
  templateUrl: './medical-service.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./medical-service.component.css']
})
export class MedicalServiceComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}

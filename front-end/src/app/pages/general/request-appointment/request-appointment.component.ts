import { AfterViewInit, Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ReloadService } from '../../../shared/service/reload.service';

@Component({
  standalone: false,
  selector: 'app-request-appointment',
  templateUrl: './request-appointment.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./request-appointment.component.css']
})
export class RequestAppointmentComponent implements OnInit,AfterViewInit {

  constructor(private reload :ReloadService) { }

  ngOnInit() {
  }
  ngAfterViewInit(): void {   
    this.reload.initializeLoader();
  }
  
}

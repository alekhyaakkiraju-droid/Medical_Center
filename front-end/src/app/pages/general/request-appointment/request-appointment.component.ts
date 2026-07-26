import { AfterViewInit, Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ReloadService } from '../../../shared/service/reload.service';
import { RouterLink } from '@angular/router';
import { AppointmentRequestComponent } from '../appointment-request/appointment-request.component';

@Component({
    selector: 'app-request-appointment',
    templateUrl: './request-appointment.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./request-appointment.component.css'],
    imports: [RouterLink, AppointmentRequestComponent]
})
export class RequestAppointmentComponent implements OnInit,AfterViewInit {

  constructor(private reload :ReloadService) { }

  ngOnInit() {
  }
  ngAfterViewInit(): void {   
    this.reload.initializeLoader();
  }
  
}

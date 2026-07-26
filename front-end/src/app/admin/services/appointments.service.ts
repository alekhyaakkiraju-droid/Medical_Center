import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { Appointment, PagedResult } from '../../pages/models';

@Injectable({
  providedIn: 'root'
})
export class AppointmentsService {

private apiUrl = `${environment.api}/Appointments`;

  constructor(private http: HttpClient , private authService :AuthServiceService) {}

  getAppointments(): Observable<PagedResult<Appointment>> {
    return this.http.get<PagedResult<Appointment>>(this.apiUrl, this.authService.getHttpOptions());
  }

}

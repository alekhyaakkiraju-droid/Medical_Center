import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { HandleErrorsService } from '../../../shared/service/handle-errors.service';

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private apiUrl = `${environment.api}/Appointments`;
  private getAppointmentsUrl = `${environment.api}/Appointments/GetAllAppointments`;

  constructor(private http: HttpClient, private authService: AuthServiceService) {}

  postAppointment(data: any): Observable<any> {
    return this.http.post(this.apiUrl, JSON.stringify(data), this.authService.getHttpOptions());
  }

  getAppointments(): Observable<any[]> {
    return this.http.get<any[]>(this.getAppointmentsUrl, this.authService.getHttpOptions());
  }

  deleteBookingById(Id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${Id}`, this.authService.getHttpOptions());
  }

  editeBooking(Id: number, updatedAppointment: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${Id}`, updatedAppointment, this.authService.getHttpOptions());
  }

  getUserAppointments(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.apiUrl}/patient/${this.authService.getNameIdentifier()}`,
      this.authService.getHttpOptions()
    );
  }

  cancelAppointment(appointmentId: string): Observable<any> {
    return this.http.put(
      `${this.apiUrl}/appointments/${appointmentId}/cancel`,
      {},
      this.authService.getHttpOptions()
    );
  }
}

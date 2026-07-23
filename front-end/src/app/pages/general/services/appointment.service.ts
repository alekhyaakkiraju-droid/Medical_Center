import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { Appointment, UpdateAppointment } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private apiUrl = `${environment.api}/Appointments`;
  private getAppointmentsUrl = `${environment.api}/Appointments/GetAllAppointments`;

  constructor(private http: HttpClient, private authService: AuthServiceService) {}

  postAppointment(data: Appointment): Observable<unknown> {
    return this.http.post(this.apiUrl, JSON.stringify(data), this.authService.getHttpOptions());
  }

  getAppointments(): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(this.getAppointmentsUrl, this.authService.getHttpOptions());
  }

  deleteBookingById(id: number): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/${id}`, this.authService.getHttpOptions());
  }

  editeBooking(id: number, updatedAppointment: UpdateAppointment): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/${id}`, updatedAppointment, this.authService.getHttpOptions());
  }

  getUserAppointments(): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(
      `${this.apiUrl}/patient/${this.authService.getNameIdentifier()}`,
      this.authService.getHttpOptions()
    );
  }

  cancelAppointment(appointmentId: string): Observable<unknown> {
    return this.http.put(
      `${this.apiUrl}/appointments/${appointmentId}/cancel`,
      {},
      this.authService.getHttpOptions()
    );
  }
}

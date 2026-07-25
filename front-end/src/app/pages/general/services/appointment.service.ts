import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Appointment, PagedResult, UpdateAppointment } from '../../models';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private apiUrl = `${environment.api}/Appointments`;
  private getAppointmentsUrl = `${environment.api}/Appointments/GetAllAppointments`;

  constructor(private http: HttpClient, private authService: AuthServiceService) {}

  private buildParams(page = 1, pageSize = 20): HttpParams {
    return new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
  }

  postAppointment(data: Appointment): Observable<unknown> {
    return this.http.post(this.apiUrl, JSON.stringify(data), this.authService.getHttpOptions());
  }

  getAppointments(page = 1, pageSize = 20): Observable<PagedResult<Appointment>> {
    return this.http.get<PagedResult<Appointment>>(
      this.getAppointmentsUrl,
      { ...this.authService.getHttpOptions(), params: this.buildParams(page, pageSize) }
    );
  }

  deleteBookingById(id: number): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/${id}`, this.authService.getHttpOptions());
  }

  editeBooking(id: number, updatedAppointment: UpdateAppointment): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/${id}`, updatedAppointment, this.authService.getHttpOptions());
  }

  getUserAppointments(page = 1, pageSize = 20): Observable<PagedResult<Appointment>> {
    return this.http.get<PagedResult<Appointment>>(
      `${this.apiUrl}/patient/${this.authService.getNameIdentifier()}`,
      { ...this.authService.getHttpOptions(), params: this.buildParams(page, pageSize) }
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

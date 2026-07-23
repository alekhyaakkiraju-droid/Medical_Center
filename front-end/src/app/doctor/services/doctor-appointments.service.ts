import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Booking, PagedResult } from '../../pages/models';

@Injectable({
  providedIn: 'root'
})
export class DoctorAppointmentsService {
  private readonly apiUrl = `${environment.api}/Doctors`;

  constructor(private http: HttpClient, private authService: AuthServiceService) {}

  getAllDoctorBookings(doctorId: string, page = 1, pageSize = 100): Observable<PagedResult<Booking>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Booking>>(
      `${this.apiUrl}/${doctorId}/bookings`,
      { ...this.authService.getHttpOptions(), params }
    );
  }

  getSpecialDoctor(doctorId: string): Observable<unknown> {
    return this.http.get<unknown>(`${this.apiUrl}/${doctorId}`, this.authService.getHttpOptions());
  }

  deleteBooking(doctorId: string, bookingId: number): Observable<unknown> {
    return this.http.delete(
      `${this.apiUrl}/${doctorId}/appointments/${bookingId}`,
      this.authService.getHttpOptions()
    );
  }
}

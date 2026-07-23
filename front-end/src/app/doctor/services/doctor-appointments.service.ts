import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DoctorAppointmentsService {
  private readonly apiUrl = `${environment.api}/Doctors`;

  constructor(private http: HttpClient, private authService: AuthServiceService) {}

  getAllDoctorBookings(doctorId: string): Observable<any> {
    return this.http.get<any[]>(`${this.apiUrl}/${doctorId}/bookings`, this.authService.getHttpOptions());
  }

  getTodayDoctorBookings(doctorId: string): Observable<any> {
    return this.http.get<any[]>(`${this.apiUrl}/${doctorId}/bookings/today`, this.authService.getHttpOptions());
  }

  getUpCommingDoctorBookings(doctorId: string): Observable<any> {
    return this.http.get<any[]>(`${this.apiUrl}/${doctorId}/bookings/UpComing`, this.authService.getHttpOptions());
  }

  getLast30DaysDoctorBookings(doctorId: string): Observable<any> {
    return this.http.get<any[]>(`${this.apiUrl}/${doctorId}/bookings/Last30Days`, this.authService.getHttpOptions());
  }

  getSpecialDoctor(doctorId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${doctorId}`, this.authService.getHttpOptions());
  }

  deleteBooking(doctorId: string, bookingId: number): Observable<any> {
    return this.http.delete(
      `${this.apiUrl}/${doctorId}/appointments/${bookingId}`,
      this.authService.getHttpOptions()
    );
  }
}

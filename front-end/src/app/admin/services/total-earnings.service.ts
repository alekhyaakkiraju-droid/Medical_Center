import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { TotalEarningsResponse } from '../../pages/models';

@Injectable({
  providedIn: 'root'
})
export class TotalEarningsService {

  private patientUrl = `${environment.api}/Appointments/total-earnings`;
  constructor(private http: HttpClient , private authService :AuthServiceService) {}

  getTotalEarnings(): Observable<TotalEarningsResponse> {
    return this.http.get<TotalEarningsResponse>(this.patientUrl, this.authService.getHttpOptions());
  }

}

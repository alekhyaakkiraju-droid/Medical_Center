import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { PagedResult } from '../../models';

export interface MedicalCenterDoctorAvailability {
  id: number;
  medicalCenterId: number;
  dayOfWeek: string;
  startTime: string;
  endTime: string;
  isAvailable: boolean;
  reasonOfUnavailability?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class DoctorAvailabilityService {
  private apiUrl = `${environment.api}/MedicalCenterDoctorAvailabilities`;

  constructor(
    private http: HttpClient,
    private authService: AuthServiceService
  ) {}

  getAvailabilities(page = 1, pageSize = 100): Observable<PagedResult<MedicalCenterDoctorAvailability>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<MedicalCenterDoctorAvailability>>(
      this.apiUrl,
      { ...this.authService.getHttpOptions(), params }
    );
  }
}

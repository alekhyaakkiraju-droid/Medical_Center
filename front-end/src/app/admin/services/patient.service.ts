import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { environment } from '../../../environments/environment';
import { PagedResult, Patient } from '../../pages/models';

@Injectable({
  providedIn: 'root'
})
export class PatientService {

  private patientUrl = `${environment.api}/Patients`;
  constructor(private http: HttpClient , private authService :AuthServiceService) {}

  getAllPatient(): Observable<PagedResult<Patient>> {
    return this.http.get<PagedResult<Patient>>(this.patientUrl, this.authService.getHttpOptions());
  }

}

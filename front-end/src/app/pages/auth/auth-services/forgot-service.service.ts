import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable } from 'rxjs';
import { AuthServiceService } from './auth-service.service';
import { HandleErrorsService } from '../../../shared/service/handle-errors.service';
import { AuthMessageResponse } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class ForgotServiceService {

  private apiUrl =  `${environment.api}/Account`; 
  
  constructor(private http: HttpClient , private authService :AuthServiceService , private handeErrorService:HandleErrorsService) {}

  forgetPassword(email: string): Observable<AuthMessageResponse> {
    const payload = { email };
    return this.http.post<AuthMessageResponse>(
      `${this.apiUrl}/forgot-password`,
      payload,
      this.authService.getHttpOptions()
    ).pipe(
      catchError(this.handeErrorService.handleError)
    );     
  }

  

}

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthServiceService } from './auth-service.service';
import { HandleErrorsService } from '../../../shared/service/handle-errors.service';
import { AuthMessageResponse } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class ResetPasswordService {

  private apiUrl = `${environment.api}/Account`;

  constructor(private http: HttpClient,
              private authService :AuthServiceService,
              private handeErrorService:HandleErrorsService
              ) {}


  resetPassword(email: string, token: string, newPassword: string): Observable<AuthMessageResponse> {
    const payload = { email, token, newPassword };
    return this.http.post<AuthMessageResponse>(
      `${this.apiUrl}/reset-password`,
      payload,
      this.authService.getHttpOptions()
    ).pipe(
      catchError(this.handeErrorService.handleError)
    );     
  }

}

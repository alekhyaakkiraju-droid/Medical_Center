import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { HandleErrorsService } from '../../../shared/service/handle-errors.service';
import { Profile, UserDetailsResponse } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {


  private apiGetUrl = `${environment.api}/Account/user-details`;  
  private apiUpdateUrl = `${environment.api}/Account/update-profile`;  


  constructor(private http: HttpClient ,
              private authService :AuthServiceService ,
              private handeErrorService :HandleErrorsService) {}

  getProfileDetails2(): Observable<UserDetailsResponse> {
    return this.http.get<UserDetailsResponse>(this.apiGetUrl, this.authService.getHttpOptions());
  }

  updateProfileDetails(profile: Profile): Observable<string> {
    return this.http.put<string>(`${this.apiUpdateUrl}`, profile, {
      ...this.authService.getHttpOptions(),
      responseType: 'text' as 'json'
    }).pipe(
      catchError((error: HttpErrorResponse) => this.handeErrorService.handleError(error))
      
    );
  }

}

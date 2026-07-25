import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { PagedResult } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class SpecializationService {

  private apiUrl = `${environment.api}/Specializations`;

  constructor(private http: HttpClient , private authService :AuthServiceService) {}

  private buildParams(page = 1, pageSize = 100): HttpParams {
    return new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
  }

  getSpecializations(page = 1, pageSize = 100): Observable<PagedResult<unknown>> {
    return this.http.get<PagedResult<unknown>>(
      this.apiUrl,
      { ...this.authService.getHttpOptions(), params: this.buildParams(page, pageSize) }
    );
  }

}

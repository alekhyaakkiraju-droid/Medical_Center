import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';
import { HandleErrorsService } from '../../../shared/service/handle-errors.service';
import { BehaviorSubject, catchError, Observable, tap } from 'rxjs';
import { Doctor, PagedResult } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class DoctorService {

private apiUrl = `${environment.api}/DoctorsWithSpectialization`;  

  constructor(private http: HttpClient ,
              private authService :AuthServiceService ,
              private handeErrorService :HandleErrorsService) {}


public doctorsSubject = new BehaviorSubject<Doctor[]>([]);
cartItems$ = this.doctorsSubject.asObservable();

  private buildParams(page = 1, pageSize = 100): HttpParams {
    return new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
  }

  getAllDoctors(page = 1, pageSize = 100): Observable<PagedResult<Doctor>> {
    return this.http.get<PagedResult<Doctor>>(
      this.apiUrl,
      { ...this.authService.getHttpOptions(), params: this.buildParams(page, pageSize) }
    ).pipe(
      tap((result: PagedResult<Doctor>) => {
        this.doctorsSubject.next(result.items ?? []);
        console.log('Doctors fetched from API:', (result.items?.length ?? 0));
      }),
      catchError(this.handeErrorService.handleError)
    );
  }
 
}

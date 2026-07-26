import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthServiceService } from '../../auth/auth-services/auth-service.service';

export interface ContactInquiryDTO {
  name: string;
  email: string;
  phone?: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class ContactService {
  private readonly apiUrl = `${environment.api}/Contact`;

  constructor(
    private http: HttpClient,
    private authService: AuthServiceService
  ) {}

  submitInquiry(data: ContactInquiryDTO): Observable<unknown> {
    return this.http.post(this.apiUrl, data, this.authService.getHttpOptions());
  }
}

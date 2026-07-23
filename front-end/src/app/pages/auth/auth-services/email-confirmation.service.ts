import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthServiceService } from './auth-service.service';

@Injectable({
  providedIn: 'root'
})
export class EmailConfirmationService {
  private apiUrl = 'http://localhost:5004/api/Account/confirm-email';

  constructor(private http: HttpClient, private authService: AuthServiceService) {}

  confirmEmail(userId: string, token: string): Observable<any> {
    const encodedToken = encodeURIComponent(token);
    const url = `${this.apiUrl}?userId=${userId}&token=${encodedToken}`;
    return this.http.get(url, {
      ...this.authService.getHttpOptions(),
      responseType: 'text'
    });
  }
}

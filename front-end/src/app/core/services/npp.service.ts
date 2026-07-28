import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';

export interface NppStatus {
  acknowledged: boolean;
  acknowledgedAt?: string;
  version: string;
}

export interface NppContent {
  content: string;
  version: string;
  lastUpdated: string;
}

@Injectable({
  providedIn: 'root',
})
export class NppService {
  private readonly baseUrl = `${environment.api}/npp`;
  private cachedStatus: NppStatus | null = null;
  private readonly statusSubject = new BehaviorSubject<NppStatus | null>(null);

  readonly status$ = this.statusSubject.asObservable();

  constructor(
    private http: HttpClient,
    private authService: AuthServiceService
  ) {}

  checkStatus(): Observable<NppStatus> {
    if (this.cachedStatus?.acknowledged) {
      return new Observable((subscriber) => {
        subscriber.next(this.cachedStatus!);
        subscriber.complete();
      });
    }

    return this.http
      .get<NppStatus>(`${this.baseUrl}/status`, this.authService.getHttpOptions())
      .pipe(
        tap((status) => {
          this.cachedStatus = status;
          this.statusSubject.next(status);
        })
      );
  }

  acknowledge(): Observable<void> {
    return this.http
      .post<void>(`${this.baseUrl}/acknowledge`, {}, this.authService.getHttpOptions())
      .pipe(
        tap(() => {
          if (this.cachedStatus) {
            this.cachedStatus = {
              ...this.cachedStatus,
              acknowledged: true,
              acknowledgedAt: new Date().toISOString(),
            };
          } else {
            this.cachedStatus = {
              acknowledged: true,
              acknowledgedAt: new Date().toISOString(),
              version: '1.0',
            };
          }

          this.statusSubject.next(this.cachedStatus);
        })
      );
  }

  getContent(): Observable<NppContent> {
    return this.http.get<NppContent>(
      `${this.baseUrl}/content`,
      this.authService.getHttpOptions()
    );
  }

  invalidateCache(): void {
    this.cachedStatus = null;
    this.statusSubject.next(null);
  }
}

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  BehaviorSubject,
  catchError,
  map,
  Observable,
  of,
  switchMap,
  throwError
} from 'rxjs';
import { tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ToastrService } from 'ngx-toastr';
import { CsrfTokenStore } from '../../../core/csrf-token.store';

export interface CurrentUser {
  userId: string;
  email: string;
  userName: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthServiceService {
  public isLoggedSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  private currentUser: CurrentUser | null = null;
  usernameTakenError = false;

  private readonly loginUrl = `${environment.api}/Account/login`;
  public readonly googleloginUrl = `${environment.api}/Account/LoginWithGoogle`;
  private readonly registerUrl = `${environment.api}/Account/register/user`;
  private readonly meUrl = `${environment.api}/Account/me`;
  private readonly logoutUrl = `${environment.api}/Account/logout`;
  private readonly antiforgeryUrl = `${environment.api}/Account/antiforgery-token`;

  constructor(
    private http: HttpClient,
    private toaster: ToastrService,
    private csrfTokenStore: CsrfTokenStore
  ) {
    this.ensureCsrfToken().subscribe();
    this.loadCurrentUser().subscribe();
  }

  ensureCsrfToken(): Observable<void> {
    return this.http.get<{ token: string }>(this.antiforgeryUrl, this.getHttpOptions()).pipe(
      tap((response) => this.csrfTokenStore.setToken(response.token)),
      map(() => void 0),
      catchError(() => of(void 0))
    );
  }

  loadCurrentUser(): Observable<CurrentUser | null> {
    return this.http.get<CurrentUser>(this.meUrl, this.getHttpOptions()).pipe(
      tap((user) => {
        this.currentUser = user;
        this.isLoggedSubject.next(true);
      }),
      catchError(() => {
        this.currentUser = null;
        this.isLoggedSubject.next(false);
        return of(null);
      })
    );
  }

  login(email: string, password: string): Observable<CurrentUser | null> {
    const loginData = { email, password };

    return this.ensureCsrfToken().pipe(
      switchMap(() =>
        this.http.post<{ expiration: string }>(this.loginUrl, loginData, this.getHttpOptions())
      ),
      switchMap(() => this.loadCurrentUser())
    );
  }

  register(
    userName: string,
    email: string,
    password: string,
    confirmPassword: string
  ): Observable<any> {
    const registerData = { userName, email, password, confirmPassword };

    return this.ensureCsrfToken().pipe(
      switchMap(() =>
        this.http.post<any>(this.registerUrl, registerData, this.getHttpOptions())
      ),
      catchError((error: any) => {
        if (error.status === 400 && error.error.includes('Username')) {
          this.usernameTakenError = true;
        }
        return throwError(() => new Error(error));
      })
    );
  }

  logout(): Observable<void> {
    return this.ensureCsrfToken().pipe(
      switchMap(() =>
        this.http.post(this.logoutUrl, {}, this.getHttpOptions())
      ),
      tap(() => {
        this.clearSession();
        this.toaster.info('Please log in again to your account');
      }),
      map(() => void 0),
      catchError(() => {
        this.clearSession();
        return of(void 0);
      })
    );
  }

  private clearSession(): void {
    this.currentUser = null;
    this.csrfTokenStore.clearToken();
    this.isLoggedSubject.next(false);
  }

  get isUserLoggedIn(): boolean {
    return this.isAuthenticated();
  }

  isAuthenticated(): boolean {
    return this.currentUser !== null;
  }

  getloggedStatus(): Observable<boolean> {
    return this.isLoggedSubject.asObservable();
  }

  isTokenExpired(): boolean {
    return !this.isAuthenticated();
  }

  isRole(role: string): boolean {
    return this.currentUser?.roles.includes(role) ?? false;
  }

  getNameIdentifier(): string | null {
    return this.currentUser?.userId ?? null;
  }

  getUserName(): string | null {
    return this.currentUser?.userName ?? null;
  }

  getUsernameFromToken(): string | null {
    return this.getUserName();
  }

  public getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json'
    });
  }

  public getHttpOptions(): { headers: HttpHeaders; withCredentials: true } {
    return {
      headers: this.getHeaders(),
      withCredentials: true
    };
  }
}

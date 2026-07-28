import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { map, Observable } from 'rxjs';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';

@Injectable({
  providedIn: 'root'
})
export class DoctorGuard implements CanActivate {

  constructor(
    private authService: AuthServiceService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean | UrlTree> {
    return this.authService.resolveSession().pipe(
      map((user) => {
        if (!user) {
          return this.router.createUrlTree(['/auth/login']);
        }
        if (this.authService.isRole('doctor')) {
          return true;
        }
        return this.router.createUrlTree(['/pages/general/errorPage']);
      })
    );
  }
}

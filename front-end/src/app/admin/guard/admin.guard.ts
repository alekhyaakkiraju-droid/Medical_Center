import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, UrlTree } from '@angular/router';
import { map, Observable } from 'rxjs';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {

  constructor(
    private authService: AuthServiceService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean | UrlTree> {
    return this.authService.resolveSession().pipe(
      map((user) => {
        if (!user) {
          return this.router.createUrlTree(['/auth/login']);
        }

        if (this.authService.isRole('admin')) {
          return true;
        }

        return this.router.createUrlTree(['/pages/general/errorPage']);
      })
    );
  }
}

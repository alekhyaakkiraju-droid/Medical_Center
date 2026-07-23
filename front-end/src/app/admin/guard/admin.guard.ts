import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {

  constructor(
    private authService: AuthServiceService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this.authService.isTokenExpired()) {
      this.router.navigate(['/auth/login']);
      return false;
    }

    if (this.authService.isRole('admin')) {
      return true;
    }

    this.router.navigate(['/pages/general/errorPage']);
    return false;
  }
}

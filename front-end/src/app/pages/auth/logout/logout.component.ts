import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthServiceService } from '../auth-services/auth-service.service';

@Component({
  standalone: false,
  selector: 'app-logout',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './logout.component.html'
})
export class LogoutComponent implements OnInit {

  constructor(private router: Router, private authService: AuthServiceService) { }


  ngOnInit() {
  }

  //--------------------logout Dialog-------------------
  
  confirmLogout(): void {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/auth/login']).then(() => {
        window.location.reload();
      });
    });
  }

  cancelLogout(): void {
    console.log('Logout cancelled.');
    this.router.navigate(['/pages/home']);
  }


}
